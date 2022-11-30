FROM mcr.microsoft.com/dotnet/sdk:6.0

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

ARG LBHPACKAGESTOKEN
ENV LBHPACKAGESTOKEN=$LBHPACKAGESTOKEN

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./AssetInformationApi.sln ./
COPY ./AssetInformationApi/AssetInformationApi.csproj ./AssetInformationApi/
COPY /NuGet.Config /root/.nuget/NuGet/NuGet.Config
RUN dotnet restore ./AssetInformationApi/AssetInformationApi.csproj  #--configfile /NuGet.Config

# Copy everything else and build
COPY . ./

RUN dotnet build -c Release -o out
RUN dotnet publish -c Release -o out

EXPOSE ${PORT:-3000}
CMD ASPNETCORE_URLS=http://+:${PORT:-3000} dotnet ./out/AssetInformationApi.dll
