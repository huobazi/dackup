#!/bin/bash
set -e

readonly USERNAME="huobazi"
readonly VERSION=`cat REVISION`
readonly IMAGE="dackup"

docker build -t ${USERNAME}/${IMAGE}:${VERSION} .
docker tag ${USERNAME}/${IMAGE}:${VERSION} ${USERNAME}/${IMAGE}:latest
docker login
docker push ${USERNAME}/${IMAGE}:${VERSION}
docker system prune
