FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
ENV service IdentityMicroservice

#Add sourcecode to Docker
ADD / /src

#Restore packages
WORKDIR /src
# RUN dotnet restore *.sln -nowarn:msb3202,nu1503 --configfile /src/config/nuget/NuGet.Config

WORKDIR /src/${service}

#Build package
FROM build AS publish

RUN dotnet publish -c Release -o /app

#Build docker image with package
FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
EXPOSE 80
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "IdentityApi.dll"]
