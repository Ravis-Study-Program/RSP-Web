FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ./RSPWebAPI.csproj ./
RUN dotnet restore RSPWebAPI.csproj
COPY . .
ARG BUILD_CONFIGURATION=Release
RUN dotnet build RSPWebAPI.csproj -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
RUN dotnet publish RSPWebAPI.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=true

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish ./publish
COPY --from=build /src ./src

COPY entrypoint.sh .
RUN chmod +x entrypoint.sh

ENTRYPOINT ["./entrypoint.sh"]
