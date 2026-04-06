# ---- Build Stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copier les fichiers projet et restaurer les dépendances
COPY BTPSecure.Shared/BTPSecure.Shared.csproj BTPSecure.Shared/
COPY BTPSecure.Client/BTPSecure.Client.csproj BTPSecure.Client/
COPY BTPSecure.Server/BTPSecure.Server.csproj BTPSecure.Server/
RUN dotnet restore BTPSecure.Server/BTPSecure.Server.csproj

# Copier tout le code source
COPY . .

# Publier en Release
RUN dotnet publish BTPSecure.Server/BTPSecure.Server.csproj -c Release -o /app/publish

# ---- Runtime Stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app

# Dépendances pour QuestPDF (génération PDF sur Linux)
RUN apt-get update && apt-get install -y --no-install-recommends \
    libfontconfig1 \
    libgdiplus \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Railway injecte PORT automatiquement
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "BTPSecure.Server.dll"]
