@echo off
REM MongoDB Detection und Conditional Deployment Script für Windows
REM Prüft ob MongoDB bereits läuft und startet entsprechende docker-compose Konfiguration

echo Pruefe ob MongoDB bereits laeuft...

REM Pruefe ob demo-campus Netzwerk existiert
docker network ls | findstr "demo-campus" >nul
if %errorlevel% neq 0 (
    echo Erstelle demo-campus Netzwerk...
    docker network create demo-campus
)

REM Pruefe lokale MongoDB (Port 27017)
netstat -an | findstr "27017" >nul
if %errorlevel% equ 0 (
    echo MongoDB laeuft bereits auf localhost:27017
    echo Pruefe ob MongoDB-Container im demo-campus Netzwerk laeuft...

    docker ps --filter "name=mongodb" --format "{{.Names}}" | findstr "mongodb" >nul
    if %errorlevel% equ 0 (
        echo MongoDB-Container gefunden - verbinde mit demo-campus Netzwerk falls noetig
        docker network connect demo-campus mongodb 2>nul
        echo Starte Backend mit vorhandenem MongoDB-Container...
        docker-compose -f docker-compose.external-mongo.yml down 2>nul
        docker-compose -f docker-compose.external-mongo.yml up --build -d
    ) else (
        echo Externe MongoDB gefunden - starte Backend mit host.docker.internal...
        REM Fallback fuer externe MongoDB
        set TEMP_MONGO_CONNECTION=mongodb://host.docker.internal:27017
        docker-compose -f docker-compose.external-mongo.yml down 2>nul
        docker-compose -f docker-compose.external-mongo.yml up --build -d
    )

) else (
    echo Keine MongoDB gefunden - starte komplettes System...
    docker-compose down 2>nul
    docker-compose up --build -d
)

echo.
echo Warte auf Services...
timeout /t 15 /nobreak >nul

echo.
echo Pruefe Backend Status...
docker ps --filter "name=dotnet-mongodb-backend" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

echo.
echo Deployment abgeschlossen!
echo Backend ist verfuegbar unter: http://localhost:8080
echo API-Endpunkte: http://localhost:8080/geoservice/poi
echo Health Check: http://localhost:8080/geoservice/health
echo MongoDB ist verfuegbar unter: mongodb://localhost:27017
echo.
echo Um Logs zu sehen: docker logs dotnet-mongodb-backend -f
echo Zum Stoppen: docker-compose down
