# StepUp - Ghid de Setup pentru Prezentare

Acest ghid te ajută să configurezi proiectul StepUp pe un laptop nou pentru prezentare.

## 📋 Cerințe Preliminare

### Backend (.NET)
- **.NET 8.0 SDK** - [Descarcă de aici](https://dotnet.microsoft.com/download/dotnet/8.0)
- Verifică instalarea: `dotnet --version` (trebuie să afișeze 8.0.x)

### Mobile (React Native / Expo)
- **Node.js** (v18 sau mai nou) - [Descarcă de aici](https://nodejs.org/)
- **npm** (vine cu Node.js)
- **Expo CLI** (se instalează global): `npm install -g expo-cli`
- Verifică instalarea:
  - `node --version`
  - `npm --version`
  - `expo --version`

### Baza de Date
- Proiectul folosește **PostgreSQL** pe Supabase (cloud)
- Connection string-ul este deja configurat în `StepUp.API/appsettings.json`
- Nu este nevoie de instalare locală a PostgreSQL

---

## 🚀 Setup Backend (API)

### 1. Navighează în folderul API
```powershell
cd StepUp.API
```

### 2. Restaurează dependențele
```powershell
dotnet restore
```

### 3. Aplică migrațiile la baza de date
```powershell
dotnet ef database update --project ../StepUp.Infrastructure --startup-project .
```

### 4. Pornește backend-ul
```powershell
dotnet run
```

**Verificare:** Backend-ul ar trebui să ruleze pe `http://localhost:5205`
- Swagger UI: `http://localhost:5205/swagger`
- Health check: `http://localhost:5205/api/health`

### 5. (Opțional) Populează baza de date cu seed data
Într-un terminal nou (backend-ul trebuie să ruleze):
```powershell
# Cleanup (șterge datele existente)
Invoke-RestMethod -Uri "http://localhost:5205/api/seed/cleanup" -Method DELETE

# Adaugă seed data
Invoke-RestMethod -Uri "http://localhost:5205/api/seed" -Method POST -ContentType "application/json"
```

**Seed data include:**
- 3 useri (Alex, Maria, Andrei)
- 2 prieteni (Alex și Maria)
- 1 cerere pending (Andrei → Alex)
- Challenge-uri și participări

---

## 📱 Setup Mobile (React Native)

### 1. Navighează în folderul Mobile
```powershell
cd StepUp.Mobile
```

### 2. Instalează dependențele
```powershell
npm install
```

### 3. **IMPORTANT: Configurează IP-ul backend-ului**

Deschide `StepUp.Mobile/config/api.js` și actualizează `API_BASE_URL` cu IP-ul laptopului tău:

```javascript
export const API_BASE_URL = __DEV__ 
  ? 'http://192.168.X.X:5205/api'  // Înlocuiește cu IP-ul laptopului tău
  : 'https://your-api-domain.com/api';
```

**Cum afli IP-ul laptopului:**
- **Windows:** Rulează `ipconfig` și caută "IPv4 Address" sub adapterul activ (Wi-Fi sau Ethernet)
- **Mac/Linux:** Rulează `ifconfig` sau `ip addr`

**Exemplu:** Dacă IP-ul este `192.168.1.100`, linia ar trebui să fie:
```javascript
? 'http://192.168.1.100:5205/api'
```

### 4. Pornește aplicația mobile
```powershell
npm start
# sau
expo start
```

**Opțiuni:**
- Apasă `a` pentru Android emulator
- Apasă `i` pentru iOS simulator
- Scanează QR-ul cu aplicația Expo Go pe telefon (același Wi-Fi cu laptopul)

---

## ✅ Verificare Finală

### Backend
1. ✅ Backend rulează pe `http://localhost:5205`
2. ✅ Swagger UI accesibil: `http://localhost:5205/swagger`
3. ✅ Health check OK: `http://localhost:5205/api/health`

### Mobile
1. ✅ Aplicația se deschide în Expo
2. ✅ Poți face login cu unul din userii din seed data
3. ✅ Poți vedea challenge-uri, prieteni, notificări

### Conturi pentru Testare (din seed data)
- **Alex Popescu:** `alex.popescu@example.com` / `Alex123!`
- **Maria Ionescu:** `maria.ionescu@example.com` / `Maria123!`
- **Andrei Georgescu:** `andrei.georgescu@example.com` / `Andrei123!`

---

## 🔧 Troubleshooting

### Backend nu pornește
- **Port 5205 ocupat:** Oprește procesul care folosește portul:
  ```powershell
  Get-NetTCPConnection -LocalPort 5205 | Select-Object -ExpandProperty OwningProcess | Stop-Process -Force
  ```
- **Eroare de migrații:** Rulează din nou:
  ```powershell
  dotnet ef database update --project ../StepUp.Infrastructure --startup-project .
  ```

### Mobile nu se conectează la backend
- **Verifică IP-ul:** Asigură-te că IP-ul din `config/api.js` este corect
- **Verifică firewall:** Permite conexiuni pe portul 5205
- **Verifică Wi-Fi:** Laptopul și telefonul trebuie să fie pe același Wi-Fi
- **Testează manual:** Deschide în browser `http://[IP-LAPTOP]:5205/api/health`

### Erori de dependențe
- **Backend:** Șterge `bin/` și `obj/`, apoi `dotnet restore` și `dotnet build`
- **Mobile:** Șterge `node_modules/`, apoi `npm install`

---

## 📝 Structura Proiectului

```
StepUp/
├── StepUp.API/              # Backend API (.NET)
│   ├── Controllers/         # API endpoints
│   ├── Middleware/          # Middleware (UpdateUserActivity)
│   └── appsettings.json     # Configurație (connection string)
├── StepUp.Application/      # Business logic
│   ├── Services/            # Servicii de business
│   ├── DTOs/                # Data Transfer Objects
│   └── Mappings/            # AutoMapper profiles
├── StepUp.Domain/           # Entități și enums
│   ├── Entities/            # User, Challenge, FriendRequest, etc.
│   └── Enums/               # Role, ChallengeStatus, etc.
├── StepUp.Infrastructure/   # Data access
│   ├── Data/                # DbContext și configurații
│   ├── Migrations/          # Migrații EF Core
│   └── Repositories/        # Repository pattern
└── StepUp.Mobile/           # Aplicație React Native
    ├── screens/             # Ecrane (Friends, Challenges, etc.)
    ├── components/          # Componente reutilizabile
    ├── config/              # Configurație API
    └── api.js               # Helper functions pentru API calls
```

---

## 🎯 Funcționalități Principale

### Backend
- ✅ Autentificare (login/register)
- ✅ Management challenge-uri (create, join, track progress)
- ✅ Sistem de prieteni (send request, accept/decline, remove)
- ✅ Notificări pentru cereri de prietenie
- ✅ Tracking activitate utilizator (LastActiveAt)
- ✅ Leaderboard și statistici

### Mobile
- ✅ Login/Register
- ✅ Lista challenge-uri (sponsored, upcoming, current)
- ✅ Detalii challenge
- ✅ Profil utilizator
- ✅ Lista prieteni cu status online/offline
- ✅ Profil prieten
- ✅ Notificări cereri prietenie
- ✅ Invitare prieteni

---

## 📞 Suport

Dacă întâmpini probleme:
1. Verifică că toate cerințele sunt instalate
2. Verifică că backend-ul rulează
3. Verifică IP-ul în `config/api.js`
4. Verifică că laptopul și telefonul sunt pe același Wi-Fi

**Succes la prezentare! 🚀**


