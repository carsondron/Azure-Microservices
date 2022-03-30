FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine3.14-amd64 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine3.14-amd64 AS build
WORKDIR /src
COPY ["src/WebApps/Portal.UI/Portal.UI.csproj", "src/WebApps/Portal.UI/"]
COPY ["src/BuildingBlocks/BuildInfoGenerator/BuildInfoGenerator.csproj", "src/BuildingBlocks/BuildInfoGenerator/"]
COPY ["src/BuildingBlocks/LoggingHandler/LoggingHandler.csproj", "src/BuildingBlocks/LoggingHandler/"]
RUN dotnet restore "src/WebApps/Portal.UI/Portal.UI.csproj"
COPY . .
WORKDIR "/src/src/WebApps/Portal.UI"
RUN dotnet build "Portal.UI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Portal.UI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Portal.UI.dll"]
