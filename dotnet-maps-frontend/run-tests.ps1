# Test-Ausführungs-Script für lokale Entwicklung

Write-Host "=== DotNet Maps Frontend Test Suite ===" -ForegroundColor Green

# Prüfe ob .NET SDK installiert ist
$dotnetVersion = dotnet --version
if ($LASTEXITCODE -ne 0) {
    Write-Host "FEHLER: .NET SDK ist nicht installiert!" -ForegroundColor Red
    exit 1
}
Write-Host "✓ .NET SDK Version: $dotnetVersion" -ForegroundColor Green

# Build das Projekt
Write-Host "`n=== Building Project ===" -ForegroundColor Cyan
dotnet build --configuration Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "FEHLER: Build fehlgeschlagen!" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Build erfolgreich" -ForegroundColor Green

# Führe Unit Tests aus
Write-Host "`n=== Running Unit Tests ===" -ForegroundColor Cyan
dotnet test Tests/DotNetMapsFrontend.Tests.csproj --configuration Release --verbosity normal --collect:"XPlat Code Coverage" --logger trx
if ($LASTEXITCODE -ne 0) {
    Write-Host "WARNUNG: Einige Unit Tests sind fehlgeschlagen!" -ForegroundColor Yellow
}

# Code Coverage Report generieren
Write-Host "`n=== Generating Code Coverage Report ===" -ForegroundColor Cyan
$coverageFiles = Get-ChildItem -Path "Tests/TestResults" -Recurse -Filter "coverage.cobertura.xml" -ErrorAction SilentlyContinue
if ($coverageFiles.Count -gt 0) {
    Write-Host "✓ Code Coverage gefunden: $($coverageFiles[0].FullName)" -ForegroundColor Green
    
    # Installiere ReportGenerator falls nicht vorhanden
    dotnet tool install --global dotnet-reportgenerator-globaltool --ignore-failed-sources 2>$null
    
    # Generiere HTML Report
    $outputDir = "Tests/CoverageReport"
    if (Test-Path $outputDir) {
        Remove-Item $outputDir -Recurse -Force
    }
    
    reportgenerator "-reports:$($coverageFiles[0].FullName)" "-targetdir:$outputDir" "-reporttypes:Html"
    Write-Host "✓ Coverage Report generiert: $outputDir/index.html" -ForegroundColor Green
} else {
    Write-Host "⚠ Keine Code Coverage Daten gefunden" -ForegroundColor Yellow
}

# Starte Anwendung für Integration Tests
Write-Host "`n=== Starting Application for Integration Tests ===" -ForegroundColor Cyan
$app = Start-Process -FilePath "dotnet" -ArgumentList "run --configuration Release --no-build" -PassThru -WindowStyle Hidden

# Warte bis Anwendung gestartet ist
Start-Sleep -Seconds 10

try {
    # Teste ob Anwendung läuft
    $response = Invoke-WebRequest -Uri "http://localhost:5148" -TimeoutSec 10 -ErrorAction SilentlyContinue
    if ($response.StatusCode -eq 200) {
        Write-Host "✓ Anwendung läuft erfolgreich auf http://localhost:5148" -ForegroundColor Green
        
        # Führe Integration Tests aus
        Write-Host "`n=== Running Integration Tests ===" -ForegroundColor Cyan
        dotnet test Tests/DotNetMapsFrontend.Tests.csproj --filter "Category=Integration" --configuration Release --verbosity normal
        
        # Führe einfache API Tests aus
        Write-Host "`n=== Running API Tests ===" -ForegroundColor Cyan
        
        # Test Map Endpoint
        try {
            $mapResponse = Invoke-WebRequest -Uri "http://localhost:5148/Map" -TimeoutSec 10
            if ($mapResponse.StatusCode -eq 200) {
                Write-Host "✓ Map Endpoint funktioniert" -ForegroundColor Green
            }
        } catch {
            Write-Host "⚠ Map Endpoint Test fehlgeschlagen: $($_.Exception.Message)" -ForegroundColor Yellow
        }
        
        # Test API Endpoint
        try {
            $apiResponse = Invoke-WebRequest -Uri "http://localhost:5148/Map/GetPointsOfInterest?lat=51.0504&lon=13.7373&radius=1000" -TimeoutSec 10
            if ($apiResponse.StatusCode -eq 200) {
                Write-Host "✓ API Endpoint funktioniert" -ForegroundColor Green
            }
        } catch {
            Write-Host "⚠ API Endpoint Test fehlgeschlagen: $($_.Exception.Message)" -ForegroundColor Yellow
        }
        
        # Test PointOfInterest List Endpoint
        try {
            $listResponse = Invoke-WebRequest -Uri "http://localhost:5148/poi" -TimeoutSec 10
            if ($listResponse.StatusCode -eq 200) {
                Write-Host "✓ PointOfInterest List Endpoint funktioniert" -ForegroundColor Green
            }
        } catch {
            Write-Host "⚠ PointOfInterest List Endpoint Test fehlgeschlagen: $($_.Exception.Message)" -ForegroundColor Yellow
        }
        
    } else {
        Write-Host "FEHLER: Anwendung antwortet nicht korrekt" -ForegroundColor Red
    }
} catch {
    Write-Host "FEHLER: Kann nicht mit der Anwendung verbinden: $($_.Exception.Message)" -ForegroundColor Red
} finally {
    # Stoppe Anwendung
    if ($app -and !$app.HasExited) {
        Stop-Process -Id $app.Id -Force
        Write-Host "✓ Anwendung gestoppt" -ForegroundColor Green
    }
}

Write-Host "`n=== Test Suite Completed ===" -ForegroundColor Green
Write-Host "Weitere Informationen:" -ForegroundColor Cyan
Write-Host "- Unit Test Results: Tests/TestResults/" -ForegroundColor Gray
Write-Host "- Coverage Report: Tests/CoverageReport/index.html" -ForegroundColor Gray
Write-Host "- Anwendung starten: dotnet run" -ForegroundColor Gray
Write-Host "- Manuelle Tests: http://localhost:5148" -ForegroundColor Gray