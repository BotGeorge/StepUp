# Ghid: Mutarea Proiectului StepUp pe Laptop pentru Prezentare

## 📋 Pași de Urmat

### 1. Copierea Proiectului
- Copiază întregul folder `StepUp` pe laptop
- Asigură-te că toate fișierele sunt copiate (inclusiv `node_modules` poate fi omis, vei reinstala)

### 2. Instalarea Dependențelor

#### Backend (API)
```powershell
cd StepUp.API
dotnet restore
```

#### Mobile (React Native/Expo)
```powershell
cd StepUp.Mobile
npm install
```

### 3. Configurarea Backend-ului

#### Opțiunea A: Folosind IP-ul local al laptopului (recomandat pentru prezentare locală)

1. **Află IP-ul laptopului:**
   - Windows: Deschide PowerShell și rulează `ipconfig`
   - Caută "IPv4 Address" sub adapterul Wi-Fi/Ethernet activ
   - Exemplu: `192.168.1.100`

2. **Configurează `StepUp.API/appsettings.json`:**
   - Nu trebuie să schimbi nimic dacă folosești baza de date Supabase (cloud)
   - Dacă vrei să folosești un ngrok tunnel nou, actualizează `AppSettings.BaseUrl`

#### Opțiunea B: Folosind ngrok (pentru acces de pe telefon peste internet)

1. **Instalează ngrok** (dacă nu este deja instalat):
   - Descarcă de la https://ngrok.com/download
   - Sau folosește: `choco install ngrok` (dacă ai Chocolatey)

2. **Pornește ngrok:**
   ```powershell
   ngrok http 5205
   ```
   - Copiază URL-ul HTTPS (ex: `https://abc123.ngrok-free.app`)

3. **Actualizează `StepUp.API/appsettings.json`:**
   ```json
   "AppSettings": {
     "BaseUrl": "https://abc123.ngrok-free.app"
   }
   ```

### 4. Configurarea Mobile App

#### Opțiunea A: Folosind IP-ul local (când telefonul și laptopul sunt pe același Wi-Fi)

Editează `StepUp.Mobile/config/network.js`:
```javascript
const HARDCODED_IP = '192.168.1.100'; // IP-ul laptopului tău
const HARDCODED_URL = null; // Lasă null
```

#### Opțiunea B: Folosind ngrok (pentru acces peste internet)

Editează `StepUp.Mobile/config/network.js`:
```javascript
const HARDCODED_IP = null; // Lasă null
const HARDCODED_URL = 'abc123.ngrok-free.app'; // URL-ul ngrok (fără https://)
```

### 5. Rularea Migrațiilor Bazei de Date

```powershell
cd StepUp.API
dotnet ef database update --project ..\StepUp.Infrastructure\StepUp.Infrastructure.csproj --startup-project .
```

### 6. Pornirea Backend-ului

```powershell
cd StepUp.API
dotnet run
```

- Backend-ul va rula pe `http://localhost:5205`
- Verifică în consolă că nu sunt erori
- Dacă folosești ngrok, asigură-te că ngrok rulează în paralel

### 7. Pornirea Mobile App

```powershell
cd StepUp.Mobile
npm start
# sau
expo start
```

- Scanează QR code-ul cu Expo Go pe telefon
- Sau apasă `a` pentru Android emulator
- Sau apasă `i` pentru iOS simulator

## 🔍 Verificări Rapide

### Verifică că backend-ul rulează:
- Deschide browser: `http://localhost:5205/api/health`
- Ar trebui să vezi un răspuns JSON

### Verifică că mobile app se conectează:
- La pornire, în consolă ar trebui să vezi: `🔗 API Base URL: http://192.168.1.100:5205/api`
- Sau: `🌐 Using hardcoded public URL: https://abc123.ngrok-free.app/api`

## ⚠️ Probleme Comune

### Mobile app nu se conectează la backend:
1. Verifică că backend-ul rulează
2. Verifică că IP-ul/URL-ul este corect în `network.js`
3. Verifică că telefonul și laptopul sunt pe același Wi-Fi (dacă folosești IP local)
4. Verifică firewall-ul Windows - poate bloca portul 5205

### Firewall Windows:
```powershell
# Deschide portul 5205 în firewall
New-NetFirewallRule -DisplayName "StepUp API" -Direction Inbound -LocalPort 5205 -Protocol TCP -Action Allow
```

### Backend nu pornește:
- Verifică că portul 5205 nu este deja folosit
- Verifică că baza de date Supabase este accesibilă
- Verifică că toate dependențele sunt instalate

## 📝 Checklist Prezentare

- [ ] Proiectul este copiat pe laptop
- [ ] Dependențele sunt instalate (backend + mobile)
- [ ] IP-ul/URL-ul este configurat corect
- [ ] Migrațiile bazei de date sunt rulate
- [ ] Backend-ul rulează fără erori
- [ ] Mobile app se conectează la backend
- [ ] Testat login/register
- [ ] Testat funcționalități principale

## 🚀 Comenzi Rapide (Copy-Paste)

```powershell
# 1. Instalare dependențe backend
cd StepUp.API
dotnet restore

# 2. Instalare dependențe mobile
cd ..\StepUp.Mobile
npm install

# 3. Rulare migrații
cd ..\StepUp.API
dotnet ef database update --project ..\StepUp.Infrastructure\StepUp.Infrastructure.csproj --startup-project .

# 4. Pornire backend (în terminal 1)
dotnet run

# 5. Pornire mobile (în terminal 2)
cd ..\StepUp.Mobile
npm start
```

## 💡 Sfaturi pentru Prezentare

1. **Testează înainte:** Rulează totul cu 1-2 zile înainte de prezentare
2. **Wi-Fi stabil:** Asigură-te că Wi-Fi-ul este stabil pentru prezentare
3. **Backup plan:** Dacă ngrok nu funcționează, folosește IP local
4. **Hotspot:** Poți folosi hotspot-ul telefonului dacă Wi-Fi-ul nu funcționează
5. **Screenshot-uri:** Fă screenshot-uri la funcționalități importante ca backup
