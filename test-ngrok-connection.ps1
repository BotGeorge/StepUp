# Script pentru testarea conexiunii ngrok
param(
    [string]$NgrokUrl = "busily-adipopexic-nancey.ngrok-free.dev"
)

Write-Host "🔍 Testare conexiune ngrok..." -ForegroundColor Cyan
Write-Host "URL: https://$NgrokUrl/api" -ForegroundColor Yellow

# Test 1: Verifică dacă ngrok răspunde
try {
    $response = Invoke-WebRequest -Uri "https://$NgrokUrl/api" -Method Get -TimeoutSec 10 -ErrorAction Stop
    Write-Host "✅ ngrok răspunde! Status: $($response.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "❌ ngrok NU răspunde!" -ForegroundColor Red
    Write-Host "   Eroare: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "💡 Verifică:" -ForegroundColor Yellow
    Write-Host "   1. Backend-ul rulează? (port 5205)" -ForegroundColor Yellow
    Write-Host "   2. ngrok rulează? (rulează 'ngrok http 5205')" -ForegroundColor Yellow
    Write-Host "   3. URL-ul ngrok este corect?" -ForegroundColor Yellow
    exit 1
}

# Test 2: Verifică dacă backend-ul local răspunde
Write-Host ""
Write-Host "🔍 Testare backend local..." -ForegroundColor Cyan
try {
    $localResponse = Invoke-WebRequest -Uri "http://localhost:5205/api" -Method Get -TimeoutSec 5 -ErrorAction Stop
    Write-Host "✅ Backend local răspunde! Status: $($localResponse.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "❌ Backend local NU răspunde!" -ForegroundColor Red
    Write-Host "   Eroare: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "💡 Pornește backend-ul: cd StepUp.API; dotnet run" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "📋 Rezumat:" -ForegroundColor Cyan
Write-Host "   URL ngrok: https://$NgrokUrl/api" -ForegroundColor White
Write-Host "   URL local: http://localhost:5205/api" -ForegroundColor White
