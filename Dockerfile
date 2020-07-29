# https://hub.docker.com/_/microsoft-dotnet-core
FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY src/dackup.csproj .
RUN dotnet restore -r linux-musl-x64

# copy and publish app and libraries
COPY src/. .
RUN dotnet publish -c release -o /app -r linux-musl-x64 --self-contained true --no-restore /p:PublishTrimmed=true /p:PublishReadyToRun=true

FROM goodsmileduck/redis-cli AS redis-cli
FROM leobueno1982/mssql-tools-alpine:1.0 AS mssql-tools

# final stage/image
FROM mcr.microsoft.com/dotnet/core/runtime-deps:3.1-alpine

# Labels
LABEL maintainer="huobazi@gmail.com"
LABEL org.label-schema.name="Dackup"
LABEL org.label-schema.description="Dackup is a free open source backup client for your files and database to Cloud "
LABEL org.label-schema.url="https://huobazi.github.io/dackup"
LABEL org.label-schema.vcs-url="https://github.com/huobazi/dackup"
LABEL org.label-schema.vendor="Marble Wu"


RUN apk --update add --no-cache postgresql-client mysql-client mongodb mongodb-tools \
  && rm -rf /var/cache/apk/*

WORKDIR /app

COPY --from=build /app .
COPY --from=redis-cli /usr/bin/redis-cli /usr/bin/redis-cli
COPY --from=mssql-tools /opt/mssql-tools/bin /opt/mssql-tools/bin

ENV PATH=$PATH:/opt/mssql-tools/bin

ENTRYPOINT ["./dackup"] 