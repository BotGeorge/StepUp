# Script pentru a marca challenge-urile expirate ca Completed
# Rulează acest script periodic sau manual pentru a curăța challenge-urile terminate

$apiUrl = "http://localhost:5205/api/challenges/mark-expired-as-completed"

Write-Host "🧹 Curățare challenge-uri expirate..." -ForegroundColor Cyan
Write-Host ""

try {
    $response = Invoke-RestMethod -Uri $apiUrl -Method POST -ContentType "application/json"
    
    if ($response.success) {
        Write-Host "✅ $($response.message)" -ForegroundColor Green
    } else {
        Write-Host "❌ Eroare: $($response.message)" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Eroare la conectare la API: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "💡 Asigură-te că backend-ul rulează pe http://localhost:5205" -ForegroundColor Yellow
}

Write-Host ""

