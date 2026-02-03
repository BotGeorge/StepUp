# Script pentru verificarea completă a conexiunii
Write-Host "🔍 Verificare conexiune StepUp..." -ForegroundColor Cyan
Write-Host ""

# 1. Verifică backend-ul local
Write-Host "1️⃣ Verificare backend local (port 5205)..." -ForegroundColor Yellow
$backendRunning = Test-NetConnection -ComputerName localhost -Port 5205 -InformationLevel Quiet -WarningAction SilentlyContinue
if ($backendRunning) {
    Write-Host "   ✅ Backend-ul rulează pe portul 5205" -ForegroundColor Green
    
    # Testează un endpoint
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5205/api/users" -Method Get -TimeoutSec 3 -ErrorAction Stop
        Write-Host "   ✅ Backend-ul răspunde la request-uri (Status: $($response.StatusCode))" -ForegroundColor Green
    } catch {
        Write-Host "   ⚠️ Backend-ul rulează dar nu răspunde corect: $($_.Exception.Message)" -ForegroundColor Yellow
    }
} else {
    Write-Host "   ❌ Backend-ul NU rulează!" -ForegroundColor Red
    Write-Host "   💡 Pornește backend-ul: cd StepUp.API; dotnet run" -ForegroundColor Yellow
}

Write-Host ""

# 2. Verifică ngrok
Write-Host "2️⃣ Verificare ngrok..." -ForegroundColor Yellow
$ngrokProcess = Get-Process | Where-Object {$_.ProcessName -like "*ngrok*"}
if ($ngrokProcess) {
    Write-Host "   ✅ ngrok rulează (PID: $($ngrokProcess.Id))" -ForegroundColor Green
} else {
    Write-Host "   ❌ ngrok NU rulează!" -ForegroundColor Red
    Write-Host "   💡 Pornește ngrok: ngrok http 5205" -ForegroundColor Yellow
}

Write-Host ""

# 3. Verifică URL-ul ngrok din network.js
Write-Host "3️⃣ Verificare configurație mobile..." -ForegroundColor Yellow
$networkJsPath = "StepUp.Mobile\config\network.js"
if (Test-Path $networkJsPath) {
    $lines = Get-Content $networkJsPath
    $hardcodedUrlLine = $lines | Where-Object { $_ -like "*HARDCODED_URL*" } | Select-Object -First 1
    if ($hardcodedUrlLine) {
        Write-Host "   📋 Linia configurării: $hardcodedUrlLine" -ForegroundColor Gray
        if ($hardcodedUrlLine -match "['`"]([^'`"]+)['`"]") {
            $ngrokUrl = $matches[1]
            if ($ngrokUrl -ne "null") {
                Write-Host "   📋 URL ngrok configurat: $ngrokUrl" -ForegroundColor Cyan
                
                # Testează conexiunea la ngrok
                try {
                    $ngrokFullUrl = if ($ngrokUrl -notmatch "^https?://") { "https://$ngrokUrl/api" } else { "$ngrokUrl/api" }
                    Write-Host "   🔍 Testare conexiune: $ngrokFullUrl" -ForegroundColor Gray
                    $response = Invoke-WebRequest -Uri $ngrokFullUrl -Method Get -TimeoutSec 10 -ErrorAction Stop
                    Write-Host "   ✅ ngrok răspunde! (Status: $($response.StatusCode))" -ForegroundColor Green
                } catch {
                    Write-Host "   ❌ ngrok NU răspunde: $($_.Exception.Message)" -ForegroundColor Red
                    Write-Host "   💡 Verifică:" -ForegroundColor Yellow
                    Write-Host "      - ngrok rulează? (ngrok http 5205)" -ForegroundColor Yellow
                    Write-Host "      - URL-ul este corect? (verifică în terminalul ngrok)" -ForegroundColor Yellow
                    Write-Host "      - Backend-ul rulează?" -ForegroundColor Yellow
                }
            } else {
                Write-Host "   ⚠️ HARDCODED_URL este setat la null" -ForegroundColor Yellow
            }
        }
    } else {
        Write-Host "   ⚠️ HARDCODED_URL nu a fost găsit în network.js" -ForegroundColor Yellow
    }
} else {
    Write-Host "   ❌ Fișierul network.js nu a fost găsit!" -ForegroundColor Red
}

Write-Host ""
Write-Host "📋 Rezumat:" -ForegroundColor Cyan
$backendStatus = if ($backendRunning) { "✅ Rulează" } else { "❌ Nu rulează" }
$ngrokStatus = if ($ngrokProcess) { "✅ Rulează" } else { "❌ Nu rulează" }
Write-Host "   Backend: $backendStatus" -ForegroundColor $(if ($backendRunning) { "Green" } else { "Red" })
Write-Host "   ngrok: $ngrokStatus" -ForegroundColor $(if ($ngrokProcess) { "Green" } else { "Red" })
