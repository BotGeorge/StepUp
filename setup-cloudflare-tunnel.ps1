# Script pentru setup Cloudflare Tunnel
# Alternativă excelentă la ngrok - gratuită, rapidă și stabilă!

Write-Host "🚀 Setup Cloudflare Tunnel pentru StepUp" -ForegroundColor Cyan
Write-Host ""

# Verifică dacă cloudflared este instalat
$cloudflaredInstalled = Get-Command cloudflared -ErrorAction SilentlyContinue

if (-not $cloudflaredInstalled) {
    Write-Host "❌ cloudflared nu este instalat!" -ForegroundColor Red
    Write-Host ""
    Write-Host "📥 Opțiuni de instalare:" -ForegroundColor Yellow
    Write-Host "   1. Cu Chocolatey: choco install cloudflared" -ForegroundColor White
    Write-Host "   2. Manual: https://github.com/cloudflare/cloudflared/releases" -ForegroundColor White
    Write-Host ""
    Write-Host "💡 Recomandare: Instalează cu Chocolatey (dacă îl ai)" -ForegroundColor Yellow
    
    $install = Read-Host "Vrei să instalez cloudflared cu Chocolatey acum? (y/n)"
    if ($install -eq "y" -or $install -eq "Y") {
        try {
            choco install cloudflared -y
            Write-Host "✅ cloudflared instalat cu succes!" -ForegroundColor Green
        } catch {
            Write-Host "❌ Eroare la instalare. Instalează manual." -ForegroundColor Red
            exit 1
        }
    } else {
        Write-Host "📥 Instalează manual cloudflared și rulează din nou acest script." -ForegroundColor Yellow
        exit 1
    }
} else {
    Write-Host "✅ cloudflared este instalat!" -ForegroundColor Green
}

Write-Host ""

# Verifică dacă backend-ul rulează
Write-Host "🔍 Verificare backend..." -ForegroundColor Yellow
$backendRunning = Test-NetConnection -ComputerName localhost -Port 5205 -InformationLevel Quiet -WarningAction SilentlyContinue

if (-not $backendRunning) {
    Write-Host "❌ Backend-ul NU rulează pe portul 5205!" -ForegroundColor Red
    Write-Host ""
    Write-Host "💡 Pornește backend-ul întâi:" -ForegroundColor Yellow
    Write-Host "   cd StepUp.API" -ForegroundColor White
    Write-Host "   dotnet run" -ForegroundColor White
    Write-Host ""
    $startBackend = Read-Host "Vrei să pornesc backend-ul acum? (y/n)"
    if ($startBackend -eq "y" -or $startBackend -eq "Y") {
        Write-Host "🚀 Pornire backend..." -ForegroundColor Yellow
        Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PWD\StepUp.API'; dotnet run" -WindowStyle Minimized
        Write-Host "⏳ Aștept 5 secunde pentru ca backend-ul să pornească..." -ForegroundColor Yellow
        Start-Sleep -Seconds 5
    } else {
        Write-Host "❌ Pornește backend-ul manual și rulează din nou acest script." -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "✅ Backend-ul rulează!" -ForegroundColor Green
}

Write-Host ""
Write-Host "🌐 Pornire Cloudflare Tunnel..." -ForegroundColor Cyan
Write-Host ""
Write-Host "📋 Instrucțiuni:" -ForegroundColor Yellow
Write-Host "   1. URL-ul public va apărea în terminal" -ForegroundColor White
Write-Host "   2. Copiază URL-ul (ex: https://abc123.trycloudflare.com)" -ForegroundColor White
Write-Host "   3. Actualizează HARDCODED_URL în StepUp.Mobile/config/network.js" -ForegroundColor White
Write-Host "   4. Repornește aplicația mobile" -ForegroundColor White
Write-Host ""
Write-Host "⚠️  Lăsă acest terminal deschis cât timp vrei ca prietenii să se conecteze!" -ForegroundColor Yellow
Write-Host ""
Write-Host "🔄 Pornire tunel..." -ForegroundColor Green
Write-Host ""

# Pornește Cloudflare Tunnel
cloudflared tunnel --url http://localhost:5205
