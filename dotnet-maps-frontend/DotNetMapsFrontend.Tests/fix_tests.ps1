# PowerShell script to add Name field to all POI objects in test files

$testFiles = @(
    "EdgeCaseTests.cs",
    "ComprehensiveTests.cs",
    "PointOfInterestControllerCrudTests.cs"
)

foreach ($file in $testFiles) {
    Write-Host "Processing $file..."
    $content = Get-Content $file -Raw
    
    # Replace patterns with Name field added
    $content = $content -replace '(\s+Category = "test",\r?\n)(\s+Details = )', '$1            Name = "Test POI",$2'
    $content = $content -replace '(\s+Category = "research_station",\r?\n)(\s+Details = )', '$1            Name = "Test Research Station",$2'
    $content = $content -replace '(\s+Category = "landmark",\r?\n)(\s+Details = )', '$1            Name = "Test Landmark",$2'
    $content = $content -replace '(\s+Category = "museum",\r?\n)(\s+Details = )', '$1            Name = "Test Museum",$2'
    $content = $content -replace '(\s+Category = "castle",\r?\n)(\s+Details = )', '$1            Name = "Test Castle",$2'
    $content = $content -replace '(\s+Category = "coffee",\r?\n)(\s+Details = )', '$1            Name = "Test Coffee Shop",$2'
    $content = $content -replace '(\s+Category = "cathedral",\r?\n)(\s+Details = )', '$1            Name = "Test Cathedral",$2'
    $content = $content -replace '(\s+Category = "",\r?\n)(\s+Details = )', '$1            Name = "Test POI",$2'
    
    # For inline initializations like { Category = "museum", Details = "Test Museum" }
    $content = $content -replace '\{ Category = "museum", Details = "Test Museum" \}', '{ Category = "museum", Name = "Test Museum", Details = "Test Museum" }'
    $content = $content -replace '\{ Category = "test", Details = "Test POI" \}', '{ Category = "test", Name = "Test POI", Details = "Test POI" }'
    $content = $content -replace '\{ Category = "cathedral", Details = ', '{ Category = "cathedral", Name = "Test Cathedral", Details = '
    
    Set-Content $file -Value $content -NoNewline
    Write-Host "Completed $file"
}

Write-Host "All files processed!"
