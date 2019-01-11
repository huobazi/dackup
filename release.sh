#!/bin/bash
set -e
usage()
{
    echo "usage: releash.sh -t your_token_content"
}

readonly RELEASE_VERSION="0.0.1.beta-1"

github_api_token=

while [ "$1" != "" ]; do
    case $1 in
        -t | --token )           shift
                                github_api_token=$1
                                ;;
        -h | --help )           usage
                                exit
                                ;;
        * )                     usage
                                exit 1
    esac
    shift
done

if [ "$github_api_token" = "" ]; then
    usage
    exit 1
fi

readonly BASE_PWD=$PWD

readonly SCRIPT_PATH="$( cd "$(dirname "$0")" ; pwd -P )"

rm -rf $SCRIPT_PATH/dist 
mkdir $SCRIPT_PATH/dist

rm -rf $SCRIPT_PATH/bin/release 

declare -a winOS=("win-x64")
declare -a unixOS=("osx-x64" "linux-x64")
declare -a releaseFiles=()

for rid in "${winOS[@]}"
do
	dotnet publish $SCRIPT_PATH/dackup.csproj -r $rid -c release
    distFile="$SCRIPT_PATH/dist/dackup-$rid.zip"
    cd $SCRIPT_PATH/bin/release/netcoreapp2.2/$rid/publish/
    zip -r $distFile .
    releaseFiles+=($distFile)
    cd $BASE_PWD
done

for rid in "${unixOS[@]}"
do 
    distFile="$SCRIPT_PATH/dist/dackup-$rid.tar.gz"
	dotnet publish $SCRIPT_PATH/dackup.csproj -r $rid -c release
    cd $SCRIPT_PATH/bin/release/netcoreapp2.2/$rid/publish/
	tar -cvzf  $distFile *
    releaseFiles+=($distFile)
    cd $BASE_PWD
done


for file in "${releaseFiles[@]}"
do
    echo "Released successfully: $file"
done


readonly owner="huobazi"
readonly repo="dackup"
readonly tag=$RELEASE_VERSION
readonly GH_API="https://api.github.com"
readonly GH_REPO="$GH_API/repos/$owner/$repo"
GH_TAGS="$GH_REPO/releases/tags/$tag"
readonly AUTH="Authorization: token $github_api_token"
readonly WGET_ARGS="--content-disposition --auth-no-challenge --no-cookie"
readonly CURL_ARGS="-LJO#"

if [[ "$tag" == 'LATEST' ]]; then
  GH_TAGS="$GH_REPO/releases/latest"
fi

# Validate token.
curl -o /dev/null -sH "$AUTH" $GH_REPO || { echo "Error: Invalid repo, token or network issue!";  exit 1; }

# Read asset tags.
response=$(curl -sH "$AUTH" $GH_TAGS)


# Get ID of the release.
eval $(echo "$response" | grep -m 1 "id.:" | grep -w id | tr : = | tr -cd '[[:alnum:]]=')
[ "$id" ] || { echo "Error: Failed to get release id for tag: $tag"; echo "$response" | awk 'length($0)<100' >&2; exit 1; }
release_id="$id"

for file in "${releaseFiles[@]}"
do
    filename=$file

    id=""
    eval $(echo "$response" | grep -C1 "name.:.\+$filename" | grep -m 1 "id.:" | grep -w id | tr : = | tr -cd '[[:alnum:]]=')
    assert_id="$id"
    if [ "$assert_id" = "" ]; then
        echo "No need to overwrite asset"
    else
        echo "Deleting asset($assert_id)... "
        curl "$GITHUB_OAUTH_BASIC" -X "DELETE" -H "Authorization: token $github_api_token" "https://api.github.com/repos/$owner/$repo/releases/assets/$assert_id"
    fi

    echo "Uploading $file to github ... "

    GH_ASSET="https://uploads.github.com/repos/$owner/$repo/releases/$release_id/assets?name=$(basename $filename)"
    
    curl "$GITHUB_OAUTH_BASIC" --data-binary @"$filename" -H "Authorization: token $github_api_token" -H "Content-Type: application/octet-stream" $GH_ASSET

    echo "Published to github successfully: $file"
done

echo "Published successfully!

cd $BASE_PWD
