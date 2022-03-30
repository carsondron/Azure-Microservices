FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine3.14-amd64 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine3.14-amd64 AS build
WORKDIR /src

# Workaround libc compatibility issues in Alpine
ENV GLIBC_REPO=https://github.com/sgerrand/alpine-pkg-glibc
ENV GLIBC_VERSION=2.28-r0
RUN set -ex && \
    apk --update add libstdc++ curl ca-certificates && \
    for pkg in glibc-${GLIBC_VERSION} glibc-bin-${GLIBC_VERSION}; \
        do curl -sSL ${GLIBC_REPO}/releases/download/${GLIBC_VERSION}/${pkg}.apk -o /tmp/${pkg}.apk; done && \
    apk add --allow-untrusted /tmp/*.apk && \
    rm -v /tmp/*.apk && \
    /usr/glibc-compat/sbin/ldconfig /lib /usr/glibc-compat/lib

COPY ["src/Services/Notifications/Notifications.Grpc/Notifications.Grpc.csproj", "src/Services/Notifications/Notifications.Grpc/"]
COPY ["src/BuildingBlocks/BuildInfoGenerator/BuildInfoGenerator.csproj", "src/BuildingBlocks/BuildInfoGenerator/"]
COPY ["src/BuildingBlocks/LoggingHandler/LoggingHandler.csproj", "src/BuildingBlocks/LoggingHandler/"]
RUN dotnet restore "src/Services/Notifications/Notifications.Grpc/Notifications.Grpc.csproj"
COPY . .
WORKDIR "/src/src/Services/Notifications/Notifications.Grpc"
RUN dotnet build "Notifications.Grpc.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Notifications.Grpc.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Notifications.Grpc.dll"]
