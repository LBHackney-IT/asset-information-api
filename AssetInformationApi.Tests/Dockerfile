FROM mcr.microsoft.com/dotnet/core/sdk:3.1

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

ENV DynamoDb_LocalMode='true'

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./AssetInformationApi.sln ./
COPY ./AssetInformationApi/AssetInformationApi.csproj ./AssetInformationApi/
COPY ./AssetInformationApi.Tests/AssetInformationApi.Tests.csproj ./AssetInformationApi.Tests/

RUN dotnet restore ./AssetInformationApi/AssetInformationApi.csproj
RUN dotnet restore ./AssetInformationApi.Tests/AssetInformationApi.Tests.csproj

# Copy everything else and build
COPY . .

RUN dotnet build -c debug -o out AssetInformationApi.Tests/AssetInformationApi.Tests.csproj

CMD dotnet test
