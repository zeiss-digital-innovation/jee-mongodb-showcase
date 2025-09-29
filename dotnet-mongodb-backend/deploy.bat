@echo off
REM MongoDB Detection und Conditional Deployment Script für Windows
REM Prüft ob MongoDB bereits läuft und startet entsprechende docker-compose Konfiguration

echo 🔍 Prüfe ob MongoDB bereits läuft...

REM Prüfe lokale MongoDB (Port 27017)
netstat -an | findstr "27017" >nul
if %errorlevel% equ 0 (
    echo ✅ MongoDB läuft bereits auf localhost:27017
    echo 🚀 Starte Backend mit externer MongoDB...

    REM Setze Umgebungsvariable für externe MongoDB
    set MONGO_CONNECTION_STRING=mongodb://host.docker.internal:27017

    REM Starte nur Backend (ohne MongoDB Service)
    docker-compose -f docker-compose.external-mongo.yml up --build

) else (
    REM Prüfe ob MongoDB-Container läuft
    docker ps --format "table {{.Names}}" | findstr "mongodb" >nul
    if %errorlevel% equ 0 (
        echo ✅ MongoDB-Container läuft bereits
        echo 🚀 Starte Backend mit vorhandenem MongoDB-Container...

        REM Starte nur Backend
        docker-compose -f docker-compose.external-mongo.yml up --build

    ) else (
        echo ❌ Keine MongoDB gefunden
        echo 🚀 Starte komplettes System Backend + MongoDB...

        REM Starte mit lokaler MongoDB
        docker-compose -f docker-compose.local.yml --profile local-db up --build
    )
)
