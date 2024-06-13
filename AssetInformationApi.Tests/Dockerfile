FROM mcr.microsoft.com/dotnet/sdk:6.0

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

ARG LBHPACKAGESTOKEN
ENV LBHPACKAGESTOKEN=$LBHPACKAGESTOKEN

ARG SONAR_TOKEN
ENV SONAR_TOKEN=$SONAR_TOKEN

ENV DynamoDb_LocalMode='true'

WORKDIR /app

# Install and run sonar cloud scanner
RUN apt-get update && apt-get install -y openjdk-17-jdk
RUN dotnet tool install --global dotnet-sonarscanner
ENV PATH="$PATH:/root/.dotnet/tools"

RUN dotnet sonarscanner begin /k:"LBHackney-IT_asset-information-api" /o:"lbhackney-it" /d:sonar.host.url=https://sonarcloud.io /d:sonar.login="${SONAR_TOKEN}"


# Copy csproj and restore as distinct layers
COPY ./AssetInformationApi.sln ./
COPY ./AssetInformationApi/AssetInformationApi.csproj ./AssetInformationApi/
COPY ./AssetInformationApi.Tests/AssetInformationApi.Tests.csproj ./AssetInformationApi.Tests/
COPY /NuGet.Config /root/.nuget/NuGet/NuGet.Config

RUN dotnet restore ./AssetInformationApi/AssetInformationApi.csproj
RUN dotnet restore ./AssetInformationApi.Tests/AssetInformationApi.Tests.csproj

# Copy everything else and build
COPY . .

RUN dotnet build -c Release -o out AssetInformationApi/AssetInformationApi.csproj
RUN dotnet build -c debug -o out AssetInformationApi.Tests/AssetInformationApi.Tests.csproj

CMD dotnet test
RUN dotnet sonarscanner end /d:sonar.login="${SONAR_TOKEN}"
