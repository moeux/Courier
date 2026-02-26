FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Courier/Courier.csproj", "Courier/"]
COPY nuget.config .

RUN --mount=type=secret,id=GITHUB_ACTOR \
    --mount=type=secret,id=GITHUB_TOKEN \
    export GITHUB_ACTOR=$(cat /run/secrets/GITHUB_ACTOR) && \
    export GITHUB_TOKEN=$(cat /run/secrets/GITHUB_TOKEN) && \
    dotnet restore "Courier/Courier.csproj" --configfile nuget.config

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
