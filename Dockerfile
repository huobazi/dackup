FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY src/dackup.csproj .
RUN dotnet restore -r linux-musl-x64

# copy and publish app and libraries
COPY src/. .
RUN dotnet publish -c release -o /app -r linux-musl-x64 --self-contained true --no-restore /p:PublishTrimmed=true /p:PublishReadyToRun=true

# final stage/image
FROM mcr.microsoft.com/dotnet/core/runtime-deps:3.1-alpine

ARG REDIS_VERSION="6.0.6"
ARG REDIS_DOWNLOAD_URL="http://download.redis.io/releases/redis-${REDIS_VERSION}.tar.gz"

ARG MSSQL_VERSION=17.5.2.1-1
ENV MSSQL_VERSION=${MSSQL_VERSION}

RUN apk update && apk upgrade \
    && apk --update add --no-cache  --virtual build-deps curl gcc make linux-headers musl-dev tar gnupg \
    && curl "$REDIS_DOWNLOAD_URL" -o redis.tar.gz  \
    && mkdir -p /usr/src/redis \
    && tar -xzf redis.tar.gz -C /usr/src/redis --strip-components=1 \
    && rm redis.tar.gz \
    && make -C /usr/src/redis install redis-cli /usr/bin \
    && rm -r /usr/src/redis \
    # Installing system utilities
    # Adding custom MS repository for mssql-tools and msodbcsql
    && curl -O https://download.microsoft.com/download/e/4/e/e4e67866-dffd-428c-aac7-8d28ddafb39b/msodbcsql17_${MSSQL_VERSION}_amd64.apk  \
    && curl -O https://download.microsoft.com/download/e/4/e/e4e67866-dffd-428c-aac7-8d28ddafb39b/mssql-tools_${MSSQL_VERSION}_amd64.apk  \
    # Verifying signature
    && curl -O https://download.microsoft.com/download/e/4/e/e4e67866-dffd-428c-aac7-8d28ddafb39b/msodbcsql17_${MSSQL_VERSION}_amd64.sig  \
    && curl -O https://download.microsoft.com/download/e/4/e/e4e67866-dffd-428c-aac7-8d28ddafb39b/mssql-tools_${MSSQL_VERSION}_amd64.sig  \
    # Importing gpg key
    && curl https://packages.microsoft.com/keys/microsoft.asc  | gpg --import -  \
    && gpg --verify msodbcsql17_${MSSQL_VERSION}_amd64.sig msodbcsql17_${MSSQL_VERSION}_amd64.apk  \
    && gpg --verify mssql-tools_${MSSQL_VERSION}_amd64.sig mssql-tools_${MSSQL_VERSION}_amd64.apk  \
    # Installing packages
    && echo y | apk add --allow-untrusted msodbcsql17_${MSSQL_VERSION}_amd64.apk mssql-tools_${MSSQL_VERSION}_amd64.apk  \
    # Deleting packages
    && rm -f msodbcsql*.sig mssql-tools*.apk \    
    && apk --update add --no-cache postgresql-client mysql-client mongodb-tools \
    && apk del build-deps \
    && rm -rf /var/cache/apk/* 

WORKDIR /app

# Labels
LABEL maintainer="huobazi@gmail.com"
LABEL org.label-schema.name="Dackup"
LABEL org.label-schema.description="Dackup is a free open source backup client for your files and database to Cloud "
LABEL org.label-schema.url="https://huobazi.github.io/dackup"
LABEL org.label-schema.vcs-url="https://github.com/huobazi/dackup"
LABEL org.label-schema.vendor="Marble Wu"

COPY --from=build /app .

ENV PATH=$PATH:/opt/mssql-tools/bin:/app
ENTRYPOINT ["./dackup"] 
