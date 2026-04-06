# ---- Build Stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copier les csproj et restaurer (layer cachee)
COPY BTPSecure.Shared/BTPSecure.Shared.csproj BTPSecure.Shared/
COPY BTPSecure.Client/BTPSecure.Client.csproj BTPSecure.Client/
COPY BTPSecure.Server/BTPSecure.Server.csproj BTPSecure.Server/
COPY BTPSecure.slnx .
RUN dotnet restore BTPSecure.Server/BTPSecure.Server.csproj

# Copier tout le code source
COPY . .

# Build puis publish separement pour eviter le bug static web assets
RUN dotnet build BTPSecure.Server/BTPSecure.Server.csproj -c Release --no-restore
RUN dotnet publish BTPSecure.Server/BTPSecure.Server.csproj -c Release -o /app/publish --no-build

# ---- Runtime Stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends \
    libfontconfig1 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "BTPSecure.Server.dll"]
