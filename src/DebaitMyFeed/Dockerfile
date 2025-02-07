FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DebaitMyFeed.Feeds/DebaitMyFeed.Feeds.csproj", "DebaitMyFeed.Feeds/"]
COPY ["DebaitMyFeed.Library/DebaitMyFeed.Library.csproj", "DebaitMyFeed.Library/"]
RUN dotnet restore "DebaitMyFeed.Feeds/DebaitMyFeed.Feeds.csproj"
COPY . .
WORKDIR "/src/DebaitMyFeed.Feeds"
RUN dotnet build "DebaitMyFeed.Feeds.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "DebaitMyFeed.Feeds.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DebaitMyFeed.Feeds.dll"]
