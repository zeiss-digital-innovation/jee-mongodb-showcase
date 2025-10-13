# 🐳 Docker Deployment - .NET MongoDB Backend

## 🚀 Übersicht

Dieses .NET Backend kann auf verschiedene Arten mit Docker deployed werden. Die intelligenten Deploy-Skripte erkennen automatisch die MongoDB-Umgebung und wählen die optimale Konfiguration.

## 📋 Deployment-Optionen

### 1. **Automatisches Deployment (Empfohlen)**

#### Windows
```cmd
# Automatische MongoDB-Erkennung und Deployment
.\deploy.bat
```

#### Linux/macOS
```bash
# Automatische MongoDB-Erkennung und Deployment
chmod +x deploy.sh
./deploy.sh
```

### 2. **Manuelle Docker-Compose Varianten**

#### Komplettes System (Backend + MongoDB)
```bash
# Startet eigene MongoDB + Backend
docker-compose up --build -d
```

#### Nur Backend (externe MongoDB)
```bash
# Nutzt vorhandene MongoDB
docker-compose -f docker-compose.external-mongo.yml up --build -d
```

#### Development Mode
```bash
# Development-Konfiguration mit Hot Reload
docker-compose -f docker-compose.local.yml up --build
```

## 🔧 Deploy-Skript Funktionalitäten

### Intelligente MongoDB-Erkennung

Die Deploy-Skripte führen automatisch folgende Prüfungen durch:

1. **Netzwerk-Erstellung**: Erstellt `demo-campus` Netzwerk falls nicht vorhanden
2. **MongoDB-Port-Check**: Prüft ob Port 27017 belegt ist
3. **Container-Erkennung**: Sucht nach laufenden MongoDB-Containern
4. **Automatische Konfiguration**: Wählt passende docker-compose Datei

### Deployment-Szenarien

| Szenario | MongoDB Status | Verwendete Konfiguration | Verhalten |
|----------|----------------|---------------------------|-----------|
| **Keine MongoDB** | Nicht gefunden | `docker-compose.yml` | Startet Backend + eigene MongoDB |
| **MongoDB Container** | Container läuft | `docker-compose.external-mongo.yml` | Nutzt vorhandenen Container |
| **Externe MongoDB** | Port 27017 belegt | `docker-compose.external-mongo.yml` | Verbindet mit externer DB |

## 🌐 Container-Konfigurationen

### Standard-Konfiguration (`docker-compose.yml`)
```yaml
services:
  backend:
    ports: ["8080:8080"]
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - MongoSettings__ConnectionString=mongodb://mongodb:27017
  
  mongodb:
    image: mongodb/mongodb-community-server:latest
    ports: ["27017:27017"]
```

### Externe MongoDB (`docker-compose.external-mongo.yml`)
```yaml
services:
  backend:
    ports: ["8080:8080"]
    environment:
      - DOTNET_RUNNING_IN_CONTAINER=true
      - MongoSettings__ConnectionString=mongodb://mongodb:27017
    networks: [demo-campus]
```

### Development Mode (`docker-compose.local.yml`)
```yaml
services:
  backend:
    ports: ["5000:80", "5001:443"]
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
    volumes:
      - ./DotNetMongoDbBackend:/app/src
```

## 🛠 Manuelle Docker-Befehle

### Image bauen
```bash
# Release Build
docker build -t dotnet-mongodb-backend .

# Development Build
docker build -t dotnet-mongodb-backend:dev --target development .
```

### Container starten
```bash
# Standard-Konfiguration
docker run -d --name backend \
  -p 8080:8080 \
  -e MongoSettings__ConnectionString=mongodb://host.docker.internal:27017 \
  dotnet-mongodb-backend

# Mit externer MongoDB
docker run -d --name backend \
  -p 8080:8080 \
  --network demo-campus \
  -e DOTNET_RUNNING_IN_CONTAINER=true \
  dotnet-mongodb-backend
```

## 🏥 Health Checks

### Container Health Check
```bash
# Health Check Status prüfen
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

# Detaillierte Health-Informationen
docker inspect --format='{{.State.Health.Status}}' dotnet-mongodb-backend
```

### API Health Check
```bash
# Backend Health Check
curl http://localhost:8080/geoservice/health

# Debug-Informationen
curl http://localhost:8080/geoservice/debug
```

## 📊 Monitoring & Debugging

### Container-Logs anzeigen
```bash
# Live-Logs verfolgen
docker logs dotnet-mongodb-backend -f

# Letzte 50 Zeilen
docker logs dotnet-mongodb-backend --tail 50

# Logs mit Timestamps
docker logs dotnet-mongodb-backend -t
```

### Container-Informationen
```bash
# Container-Details
docker inspect dotnet-mongodb-backend

# Netzwerk-Informationen
docker network inspect demo-campus

# Resource-Verbrauch
docker stats dotnet-mongodb-backend
```

## 🔄 Container-Management

### Stoppen und Neustarten
```bash
# Container stoppen
docker-compose down

# Container neustarten
docker-compose restart backend

# Komplettes System neu bauen
docker-compose down
docker-compose up --build -d
```

### Daten-Volumes verwalten
```bash
# Volumes anzeigen
docker volume ls

# MongoDB-Daten löschen
docker volume rm mongo-data

# Alle ungenutzten Volumes löschen
docker volume prune
```

## 🌍 Umgebungsvariablen

### Basis-Konfiguration
```bash
# MongoDB-Verbindung
MONGO_CONNECTION_STRING="mongodb://localhost:27017"
MongoSettings__Database="demo-campus"
MongoSettings__Collections__Pois="point-of-interest"

# ASP.NET Core
ASPNETCORE_ENVIRONMENT="Production"
ASPNETCORE_URLS="http://+:8080"
DOTNET_RUNNING_IN_CONTAINER="true"
```

### Erweiterte Konfiguration
```bash
# Logging-Level
Logging__LogLevel__Default="Information"
Logging__LogLevel__DotNetMongoDbBackend="Debug"

# MongoDB-Timeouts
MongoSettings__ConnectionTimeout="5000"
MongoSettings__ServerSelectionTimeout="5000"
MongoSettings__SocketTimeout="5000"
```

## 🔗 Netzwerk-Konfiguration

### Demo-Campus Netzwerk
```bash
# Netzwerk erstellen
docker network create demo-campus

# Container zum Netzwerk hinzufügen
docker network connect demo-campus mongodb
docker network connect demo-campus dotnet-mongodb-backend

# Netzwerk-Details anzeigen
docker network inspect demo-campus
```

### Port-Mapping
| Service | Container Port | Host Port | Protokoll |
|---------|----------------|-----------|-----------|
| Backend (Prod) | 8080 | 8080 | HTTP |
| Backend (Dev) | 80, 443 | 5000, 5001 | HTTP, HTTPS |
| MongoDB | 27017 | 27017 | TCP |

## 🚧 Troubleshooting

### Häufige Probleme

#### MongoDB-Verbindungsfehler
```bash
# MongoDB-Status prüfen
docker ps --filter "name=mongodb"

# Netzwerk-Konnektivität testen
docker exec dotnet-mongodb-backend curl -f mongodb:27017

# MongoDB-Logs überprüfen
docker logs mongodb
```

#### Port-Konflikte
```bash
# Verwendete Ports anzeigen
netstat -an | findstr 8080  # Windows
lsof -i :8080              # Linux/macOS

# Container mit anderem Port starten
docker run -p 8081:8080 dotnet-mongodb-backend
```

#### Build-Probleme
```bash
# Cache löschen und neu bauen
docker system prune -f
docker-compose build --no-cache backend

# .NET Restore-Probleme beheben
docker run --rm -v $(pwd):/app mcr.microsoft.com/dotnet/sdk:9.0 dotnet restore /app/DotNetMongoDbBackend
```

## 📈 Performance-Optimierung

### Multi-Stage Dockerfile
```dockerfile
# Optimierte Dockerfile-Struktur
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY *.csproj .
RUN dotnet restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DotNetMongoDbBackend.dll"]
```

### Resource-Limits
```yaml
services:
  backend:
    deploy:
      resources:
        limits:
          memory: 512M
          cpus: '0.5'
        reservations:
          memory: 256M
          cpus: '0.25'
```

## 🎯 Best Practices

1. **Immer deploy.bat/deploy.sh verwenden** für automatische Konfiguration
2. **Health Checks aktivieren** für Production-Deployments
3. **Logs-Rotation konfigurieren** für langfristige Deployments
4. **Secrets management** für Produktionsumgebungen
5. **Backup-Strategien** für MongoDB-Daten implementieren

## 🔒 Sicherheit

### Production-Deployment
```bash
# Sichere MongoDB-Verbindung
export MONGO_CONNECTION_STRING="mongodb://user:password@mongodb:27017/demo-campus?authSource=admin"

# HTTPS aktivieren
export ASPNETCORE_URLS="https://+:443;http://+:80"
export ASPNETCORE_Kestrel__Certificates__Default__Path="/app/cert.pfx"
```

### Netzwerk-Isolation
```yaml
networks:
  backend-network:
    driver: bridge
    internal: true  # Kein Internetzugang
  frontend-network:
    driver: bridge
```

---

**Powered by Docker & .NET** 🐳⚡
