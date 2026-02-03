# ⚡ Soluție Rapidă: Cloudflare Tunnel

**Cea mai rapidă și stabilă alternativă la ngrok!**

## 🚀 Pași (2 minute)

### 1. Instalează cloudflared

**Opțiunea A - Cu Chocolatey (recomandat):**
```powershell
choco install cloudflared
```

**Opțiunea B - Manual:**
- Descarcă de la: https://github.com/cloudflare/cloudflared/releases
- Extrage `cloudflared.exe` într-un folder din PATH

### 2. Pornește backend-ul

```powershell
cd StepUp.API
dotnet run
```

### 3. Pornește Cloudflare Tunnel

**Într-un terminal nou:**
```powershell
cloudflared tunnel --url http://localhost:5205
```

**Sau folosește scriptul:**
```powershell
.\setup-cloudflare-tunnel.ps1
```

### 4. Copiază URL-ul

Vei vedea ceva de genul:
```
+--------------------------------------------------------------------------------------------+
|  Your quick Tunnel has been created! Visit it at (it may take some time to be reachable): |
|  https://abc123-def456.trycloudflare.com                                                   |
+--------------------------------------------------------------------------------------------+
```

### 5. Actualizează aplicația mobile

Deschide `StepUp.Mobile/config/network.js` și actualizează:
```javascript
const HARDCODED_URL = 'abc123-def456.trycloudflare.com'; // Fără https://
```

### 6. Repornește aplicația mobile

Aplicația va folosi automat URL-ul Cloudflare!

---

## ✅ Avantaje față de ngrok

- ✅ **Mai rapid** - infrastructură Cloudflare
- ✅ **Mai stabil** - mai puține timeout-uri
- ✅ **URL-ul rămâne același** cât timp rulează
- ✅ **100% gratuit** - fără limitări
- ✅ **Fără autentificare** necesară pentru tuneluri rapide

---

## 🎯 Gata!

Prietenii tăi pot acum să se conecteze folosind același URL, chiar dacă nu sunt pe același Wi-Fi!

**Notă:** URL-ul se schimbă la fiecare restart al tunelului. Pentru URL permanent, vezi **Railway.app** în `SOLUTII_CONECTARE_REMOTA.md`.
