# https://hub.docker.com/_/microsoft-dotnet-core
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY src/*.csproj .
RUN dotnet restore -r linux-x64

# copy and publish app and libraries
COPY src/. .
RUN dotnet publish -c release -o /app -r linux-x64 --self-contained true --no-restore /p:PublishTrimmed=true /p:PublishReadyToRun=true

# final stage/image
FROM mcr.microsoft.com/dotnet/core/runtime-deps:3.1-bionic
RUN apt-get clean && apt-get update && apt-get install -y wget gnupg \	
    && wget -qO - https://www.mongodb.org/static/pgp/server-4.2.asc |  apt-key add - \
    &&  echo "deb [ arch=amd64 ] https://repo.mongodb.org/apt/ubuntu bionic/mongodb-org/4.0 multiverse"  | tee /etc/apt/sources.list.d/mongodb-org-4.0.list \
    && apt-key adv --keyserver keyserver.ubuntu.com --recv-keys 68818C72E52529D4 \
    && apt-get update \
    && apt-get install -y mongodb-org-tools postgresql-client mysql-client openssh-client apt-transport-https ca-certificates software-properties-common \
    && update-ca-certificates \    
    && cd /tmp \
    && wget http://download.redis.io/redis-stable.tar.gz  \
    && tar xvzf redis-stable.tar.gz \
    && cd redis-stable \
    && make \
    && cp src/redis-cli /usr/local/bin/ \
    && chmod 755 /usr/local/bin/redis-cli \
    && rm -rf /tmp/redis-stable  \
    && rm -rf /var/lib/apt/lists/*


WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["./dackup"]
