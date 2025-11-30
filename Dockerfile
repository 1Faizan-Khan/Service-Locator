# --- Build stage ---
ARG DOTNET_VERSION=7.0
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src

# Copy solution and project files
COPY *.sln ./
# Copy project folders (adjust ServiceLocator/ if needed)
COPY ServiceLocator/ServiceLocator.csproj ./ServiceLocator/
# Restore
RUN dotnet restore

# Copy the rest of the sources
COPY . .

# Publish
WORKDIR /src/ServiceLocator
RUN dotnet publish -c Release -o /app/publish

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS runtime
WORKDIR /app

# Set ASP.NET to listen on port 10000 (Render commonly uses PORT env but this is explicit)
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

# Copy published app
COPY --from=build /app/publish .

# Entrypoint
ENTRYPOINT ["dotnet", "ServiceLocator.dll"]