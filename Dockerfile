# ---- Build Stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copier TOUT d'un coup (moins de layers = plus rapide)
COPY . .

# Restore + Publish en une seule commande — desactiver le trim et la compression Blazor pour accelerer
RUN dotnet publish BTPSecure.Server/BTPSecure.Server.csproj \
    -c Release \
    -o /app/publish \
    -p:BlazorEnableCompression=false \
    -p:PublishTrimmed=false

# ---- Runtime Stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app

# QuestPDF a besoin de libfontconfig sur Linux
RUN apt-get update && apt-get install -y --no-install-recommends \
    libfontconfig1 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "BTPSecure.Server.dll"]
