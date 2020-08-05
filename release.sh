#!/bin/bash
set -e

readonly RELEASE_VERSION=`cat REVISION`

usage()
{
    echo "---------------------------------------"
    echo "usage:"
    echo "releash.sh -t your_token_content"
    echo "---------------------------------------"
}

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

readonly BASE_PWD=$PWD
readonly SCRIPT_PATH="$( cd "$(dirname "$0")" ; pwd -P )"
readonly SRC_PATH=$SCRIPT_PATH/src
readonly PROJECT_PATH=$SRC_PATH/dackup.csproj
readonly DOTNET_CORE_APP_VERSION_FOLDER=$SRC_PATH/bin/Release/netcoreapp3.1

dotnet nuget locals all --clear

rm -rf $SCRIPT_PATH/dist 
mkdir $SCRIPT_PATH/dist
rm -rf $SRC_PATH/bin/Release 

declare -a winOS=("win10-x64" "win81-x64" "win7-x64" "win-x64")
declare -a unixOS=("osx-x64" "linux-x64")
declare -a releaseFiles=()

for rid in "${winOS[@]}"
do
    distFile="$SCRIPT_PATH/dist/dackup-$rid.zip"
    dotnet publish $PROJECT_PATH -c Release -p:PublishSingleFile=true -p:IncludeSymbolsInSingleFile=true -r $rid 
    cd $DOTNET_CORE_APP_VERSION_FOLDER/$rid/publish/
    zip -r $distFile .
    releaseFiles+=($distFile)
    cd $BASE_PWD
done

for rid in "${unixOS[@]}"
do
    distFile="$SCRIPT_PATH/dist/dackup-$rid.tar.gz"
    dotnet publish $PROJECT_PATH -c Release -p:PublishSingleFile=true -p:IncludeSymbolsInSingleFile=true -r $rid 
    cd $DOTNET_CORE_APP_VERSION_FOLDER/$rid/publish/
    tar -cvzf  $distFile *
    releaseFiles+=($distFile)
    cd $BASE_PWD
done

for file in "${releaseFiles[@]}"
do
    echo "Released successfully: $file"
done

if [ "$github_api_token" = "" ]; then
    exit 0
fi

readonly owner="huobazi"
readonly repo="dackup"
readonly tag=$RELEASE_VERSION

# https://github.com/tcnksm/ghr

for file in "${releaseFiles[@]}"
do
    ghr -t $github_api_token  -u $owner  -r $repo -n "Dackup $tag" -replace $tag $file
    echo "$file upload to github successfully."
done

echo "Published successfully!"

cd $BASE_PWD
