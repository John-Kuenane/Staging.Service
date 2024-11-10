#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Services/ISSN/ISSN.API/ISSN.API.csproj", "Services/ISSN/ISSN.API/"]
COPY ["BuildingBlocks/EventBus/EventBusRabbitMQ/EventBusRabbitMQ.csproj", "BuildingBlocks/EventBus/EventBusRabbitMQ/"]
COPY ["BuildingBlocks/EventBus/EventBus/EventBus.csproj", "BuildingBlocks/EventBus/EventBus/"]
COPY ["BuildingBlocks/EventBus/IntegrationEventLogEF/IntegrationEventLogEF.csproj", "BuildingBlocks/EventBus/IntegrationEventLogEF/"]
COPY ["BuildingBlocks/ServiceDefaults/ServiceDefaults.csproj", "BuildingBlocks/ServiceDefaults/"]
COPY ["Services/ISSN/ISSN.Domain/ISSN.Domain.csproj", "Services/ISSN/ISSN.Domain/"]
COPY ["Services/ISSN/ISSN.Infrastructure/ISSN.Infrastructure.csproj", "Services/ISSN/ISSN.Infrastructure/"]
RUN dotnet restore "./Services/ISSN/ISSN.API/ISSN.API.csproj"
COPY . .
WORKDIR "/src/Services/ISSN/ISSN.API"
RUN dotnet build "./ISSN.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ISSN.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ISSN.API.dll"]