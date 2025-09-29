# PowerShell script to display MongoDB container network information

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "MongoDB Container Network Information" -ForegroundColor Cyan  
Write-Host "==========================================" -ForegroundColor Cyan

$containerName = "mongodb-demo-campus"

# Check if container is running
$containerRunning = docker ps -q -f "name=$containerName"

if ($containerRunning) {
    Write-Host "Container Status: Running ✅" -ForegroundColor Green
    
    # Get IP address
    $ipAddress = docker inspect $containerName --format '{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}'
    Write-Host "IP Address: $ipAddress" -ForegroundColor Yellow
    
    # Get detailed network information
    $networkInfo = docker inspect $containerName --format '{{json .NetworkSettings.Networks}}' | ConvertFrom-Json
    
    Write-Host ""
    Write-Host "Network Details:" -ForegroundColor White
    foreach ($network in $networkInfo.PSObject.Properties) {
        $networkName = $network.Name
        $config = $network.Value
        
        Write-Host "  Network: $networkName" -ForegroundColor Magenta
        Write-Host "  IP: $($config.IPAddress)" -ForegroundColor Yellow  
        Write-Host "  Gateway: $($config.Gateway)" -ForegroundColor Yellow
        Write-Host "  Aliases: $($config.Aliases -join ', ')" -ForegroundColor Yellow
        Write-Host "  DNS Names: $($config.DNSNames -join ', ')" -ForegroundColor Yellow
        Write-Host ""
    }
    
    Write-Host "JEE Backend Connection Options:" -ForegroundColor White
    Write-Host "  - mongodb://mongodb-demo-campus:27017/demo-campus (recommended)" -ForegroundColor Green
    Write-Host "  - mongodb://mongodb:27017/demo-campus (short alias)" -ForegroundColor Green  
    Write-Host "  - mongodb://$ipAddress:27017/demo-campus (IP - not recommended)" -ForegroundColor Red
    
} else {
    Write-Host "Container Status: Not running ❌" -ForegroundColor Red
    Write-Host "Start the container with: docker-compose up -d" -ForegroundColor Yellow
}

Write-Host "==========================================" -ForegroundColor Cyan