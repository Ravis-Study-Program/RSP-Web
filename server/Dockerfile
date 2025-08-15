# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ./RSPWebAPI.csproj ./

RUN dotnet restore RSPWebAPI.csproj
COPY . .

ARG BUILD_CONFIGURATION=Release
ARG BUILD_ENVIRONMENT=Production
RUN dotnet build RSPWebAPI.csproj -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish RSPWebAPI.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=true

# Production runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS production
WORKDIR /app

RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

COPY entrypoint.sh .
RUN chmod +x entrypoint.sh

EXPOSE 4000

HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
    CMD curl -f http://localhost:4000/health || exit 1

ENTRYPOINT ["./entrypoint.sh"]

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS development
WORKDIR /app

RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY entrypoint.sh .
RUN chmod +x entrypoint.sh

EXPOSE 4000

HEALTHCHECK --interval=10s --timeout=5s --start-period=30s --retries=10 \
    CMD curl -f http://localhost:4000/health || exit 1

ENTRYPOINT ["./entrypoint.sh"]
