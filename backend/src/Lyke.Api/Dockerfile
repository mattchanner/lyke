# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["backend/src/Lyke.Api/Lyke.Api.csproj", "backend/src/Lyke.Api/"]
COPY ["backend/src/Lyke.Application/Lyke.Application.csproj", "backend/src/Lyke.Application/"]
COPY ["aspire/Lyke.ServiceDefaults/Lyke.ServiceDefaults.csproj", "aspire/Lyke.ServiceDefaults/"]
COPY ["backend/src/Lyke.Core/Lyke.Core.csproj", "backend/src/Lyke.Core/"]
COPY ["backend/src/Lyke.Infrastructure/Lyke.Infrastructure.csproj", "backend/src/Lyke.Infrastructure/"]
RUN dotnet restore "./backend/src/Lyke.Api/Lyke.Api.csproj"
COPY . .
WORKDIR "/src/backend/src/Lyke.Api"
RUN dotnet build "./Lyke.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

RUN apt-get -y update && apt-get -y upgrade && apt-get install -y ffmpeg

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Lyke.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Lyke.Api.dll"]