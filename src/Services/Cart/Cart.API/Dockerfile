FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine3.14-amd64 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine3.14-amd64 AS build
WORKDIR /src
COPY ["src/Services/Cart/Cart.API/Cart.API.csproj", "src/Services/Cart/Cart.API/"]
COPY ["src/BuildingBlocks/EventBusMessages/EventBusMessages.csproj", "src/BuildingBlocks/EventBusMessages/"]
COPY ["src/BuildingBlocks/BuildInfoGenerator/BuildInfoGenerator.csproj", "src/BuildingBlocks/BuildInfoGenerator/"]
COPY ["src/BuildingBlocks/LoggingHandler/LoggingHandler.csproj", "src/BuildingBlocks/LoggingHandler/"]
RUN dotnet restore "src/Services/Cart/Cart.API/Cart.API.csproj"
COPY . .
WORKDIR "/src/src/Services/Cart/Cart.API"
RUN dotnet build "Cart.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Cart.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cart.API.dll"]
