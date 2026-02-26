FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Courier/Courier.csproj", "Courier/"]
COPY nuget.config .

RUN --mount=type=secret,id=github_token \
    --mount=type=secret,id=github_actor \
    dotnet restore "Courier/Courier.csproj" \
        --configfile nuget.config \
        -p:Username="$(cat /run/secrets/github_actor)" \
        -p:Password="$(cat /run/secrets/github_token)"
        
COPY . .
WORKDIR "/src/Courier"
RUN dotnet build "./Courier.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Courier.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Courier.dll"]
