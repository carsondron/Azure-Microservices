FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine3.14-amd64 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine3.14-amd64 AS build
WORKDIR /src

# Workaround libc compatibility issues in Alpine
ENV \
    GLIBC_REPO=https://github.com/sgerrand/alpine-pkg-glibc \
    GLIBC_VERSION=2.28-r0
RUN set -ex && \
    apk --update add libstdc++ curl ca-certificates icu-libs && \
    for pkg in glibc-${GLIBC_VERSION} glibc-bin-${GLIBC_VERSION}; \
        do curl -sSL ${GLIBC_REPO}/releases/download/${GLIBC_VERSION}/${pkg}.apk -o /tmp/${pkg}.apk; done && \
    apk add --allow-untrusted /tmp/*.apk && \
    rm -v /tmp/*.apk && \
    /usr/glibc-compat/sbin/ldconfig /lib /usr/glibc-compat/lib

COPY ["src/Services/Orders/Orders.API/Orders.API.csproj", "src/Services/Orders/Orders.API/"]
COPY ["src/BuildingBlocks/EventBusMessages/EventBusMessages.csproj", "src/BuildingBlocks/EventBusMessages/"]
COPY ["src/BuildingBlocks/LoggingHandler/LoggingHandler.csproj", "src/BuildingBlocks/LoggingHandler/"]
COPY ["src/BuildingBlocks/BuildInfoGenerator/BuildInfoGenerator.csproj", "src/BuildingBlocks/BuildInfoGenerator/"]
COPY ["src/Services/Orders/Orders.Application/Orders.Application.csproj", "src/Services/Orders/Orders.Application/"]
COPY ["src/Services/Orders/Orders.Domain/Orders.Domain.csproj", "src/Services/Orders/Orders.Domain/"]
COPY ["src/Services/Orders/Orders.Infrastructure/Orders.Infrastructure.csproj", "src/Services/Orders/Orders.Infrastructure/"]

RUN dotnet restore "src/Services/Orders/Orders.API/Orders.API.csproj"
COPY . .
WORKDIR "/src/src/Services/Orders/Orders.API"

RUN ls -ltr
RUN ls -ltr ../Orders.Application/
RUN dotnet build "Orders.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Orders.API.csproj" -c Release -o /app/publish

FROM base AS final

# Enable globalization APIs
ENV \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    LC_ALL=en_US.UTF-8 \
    LANG=en_US.UTF-8

RUN set -ex && \
    apk add --no-cache icu-libs

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Orders.API.dll"]
