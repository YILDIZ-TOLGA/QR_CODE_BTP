FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends \
    libfontconfig1 \
    && rm -rf /var/lib/apt/lists/*

COPY publish/ .

ENV ASPNETCORE_ENVIRONMENT=Production

# Utiliser un shell pour que $PORT soit interprété au runtime
CMD ["sh", "-c", "dotnet BTPSecure.Server.dll --urls http://+:${PORT:-8080}"]
