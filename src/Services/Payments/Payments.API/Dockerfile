FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine3.14-amd64 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine3.14-amd64 AS build
WORKDIR /src
COPY ["src/Services/Payments/Payments.API/Payments.API.csproj", "src/Services/Payments/Payments.API/"]
COPY ["src/BuildingBlocks/EventBusMessages/EventBusMessages.csproj", "src/BuildingBlocks/EventBusMessages/"]
COPY ["src/BuildingBlocks/BuildInfoGenerator/BuildInfoGenerator.csproj", "src/BuildingBlocks/BuildInfoGenerator/"]
COPY ["src/BuildingBlocks/LoggingHandler/LoggingHandler.csproj", "src/BuildingBlocks/LoggingHandler/"]
COPY ["src/BuildingBlocks/TelemetryKitchenSink/TelemetryKitchenSink.csproj", "src/BuildingBlocks/TelemetryKitchenSink/"]
RUN dotnet restore "src/Services/Payments/Payments.API/Payments.API.csproj"
COPY . .
WORKDIR "/src/src/Services/Payments/Payments.API"
RUN dotnet build "Payments.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Payments.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Payments.API.dll"]
