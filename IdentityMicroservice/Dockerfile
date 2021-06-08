FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src

COPY IdentityMicroservice/IdentityApi.csproj IdentityMicroservice/
COPY IdentityCore/IdentityCore.csproj IdentityCore/
COPY IdentityInfrastructure/IdentityInfrastructure.csproj IdentityInfrastructure/
COPY NuGet.config ./

RUN dotnet restore IdentityMicroservice/IdentityApi.csproj
COPY . .

WORKDIR /src/IdentityMicroservice
RUN dotnet build IdentityApi.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish IdentityApi.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "IdentityApi.dll"]
