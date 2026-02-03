# Script pentru cleanup și seed data
$baseUrl = "http://localhost:5205"

Write-Host "=== StepUp Seed Data Script ===" -ForegroundColor Cyan
Write-Host ""

# 1. Cleanup
Write-Host "1. Running cleanup..." -ForegroundColor Yellow
try {
    $cleanupResponse = Invoke-RestMethod -Uri "$baseUrl/api/seed/cleanup" -Method Delete
    Write-Host "   ✓ Cleanup success: $($cleanupResponse.message)" -ForegroundColor Green
} catch {
    Write-Host "   ✗ Cleanup failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "   (This is OK if endpoint doesn't exist or backend needs restart)" -ForegroundColor Gray
}
Write-Host ""

# 2. Seed
Write-Host "2. Running seed..." -ForegroundColor Yellow
try {
    $seedResponse = Invoke-RestMethod -Uri "$baseUrl/api/seed" -Method Post -ContentType "application/json"
    Write-Host "   ✓ Seed success: $($seedResponse.message)" -ForegroundColor Green
    Write-Host "   ✓ Total challenges created: $($seedResponse.data.totalChallenges)" -ForegroundColor Green
} catch {
    Write-Host "   ✗ Seed failed: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response.StatusCode -eq 500) {
        Write-Host "   (Try running cleanup first, or restart the backend)" -ForegroundColor Gray
    }
}
Write-Host ""
Write-Host "=== Done ===" -ForegroundColor Cyan

