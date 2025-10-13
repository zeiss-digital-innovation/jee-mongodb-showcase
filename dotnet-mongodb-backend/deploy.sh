#!/bin/bash

# MongoDB Detection und Conditional Deployment Script
# Prüft ob MongoDB bereits läuft und startet entsprechende docker-compose Konfiguration

echo "🔍 Prüfe ob MongoDB bereits läuft..."

# Prüfe lokale MongoDB (Port 27017)
if nc -z localhost 27017 2>/dev/null; then
    echo "✅ MongoDB läuft bereits auf localhost:27017"
    echo "🚀 Starte Backend mit externer MongoDB..."

    # Setze Umgebungsvariable für externe MongoDB
    export MONGO_CONNECTION_STRING="mongodb://host.docker.internal:27017"

    # Starte nur Backend (ohne MongoDB Service)
    docker-compose -f docker-compose.external-mongo.yml up --build

elif docker ps --format "table {{.Names}}" | grep -q "mongodb"; then
    echo "✅ MongoDB-Container läuft bereits"
    echo "🚀 Starte Backend mit vorhandenem MongoDB-Container..."

    # Hole MongoDB Container IP
    MONGO_IP=$(docker inspect -f '{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}' mongodb)
    export MONGO_CONNECTION_STRING="mongodb://$MONGO_IP:27017"

    # Starte nur Backend
    docker-compose -f docker-compose.external-mongo.yml up --build

else
    echo "❌ Keine MongoDB gefunden"
    echo "🚀 Starte komplettes System (Backend + MongoDB)..."

    # Starte mit lokaler MongoDB
    docker-compose up --build
fi

echo ""
echo "Deployment abgeschlossen!"
echo "Backend ist verfügbar unter: http://localhost:5000"
echo "MongoDB ist verfügbar unter: mongodb://localhost:27017"
echo ""
echo "Um Logs zu sehen: docker-compose logs -f"
echo "Zum Stoppen: docker-compose down"
