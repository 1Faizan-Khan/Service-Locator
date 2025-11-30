# --- Build stage ---
ARG DOTNET_VERSION=9.0
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src

# Copy everything into the build context
COPY . .

# Restore (dotnet will pick up ServiceLocator.csproj at /src)
RUN dotnet restore

# Publish the web project (adjust filename if your .csproj name differs)
RUN dotnet publish ServiceLocator.csproj -c Release -o /app/publish

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ServiceLocator.dll"]