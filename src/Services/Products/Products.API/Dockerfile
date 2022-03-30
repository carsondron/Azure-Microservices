FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine3.14-amd64 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine3.14-amd64 AS build
WORKDIR /src
COPY ["src/Services/Products/Products.API/Products.API.csproj", "src/Services/Products/Products.API/"]
COPY ["src/BuildingBlocks/BuildInfoGenerator/BuildInfoGenerator.csproj", "src/BuildingBlocks/BuildInfoGenerator/"]
COPY ["src/BuildingBlocks/LoggingHandler/LoggingHandler.csproj", "src/BuildingBlocks/LoggingHandler/"]
RUN dotnet restore "src/Services/Products/Products.API/Products.API.csproj"
COPY . .
WORKDIR "/src/src/Services/Products/Products.API"
RUN dotnet build "Products.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Products.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Products.API.dll"]
