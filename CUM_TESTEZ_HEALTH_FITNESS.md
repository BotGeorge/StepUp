# 🧪 Cum să Testezi Health/Fitness Integration Fără Device Fizic

## 📱 Opțiuni de Testare

### 1. **Modul MOCK (Recomandat pentru Development)**

Aplicația rulează automat în **modul MOCK** când:
- Nu ai biblioteca `react-native-health` instalată
- Nu ai permisiuni de health
- Rulezi în development mode

**Cum funcționează:**
- Generează date random realiste (pași, calorii, distanță)
- Salvează datele local în AsyncStorage
- Poți seta manual date mock din aplicație
- Datele mock sunt trimise la backend la fel ca datele reale

**Pași pentru testare:**

1. **Deschide aplicația** - Modul MOCK este activ automat
2. **Mergi în Profil** - Vei vedea "Sincronizează Acum (Mock Mode)"
3. **Setează Date Mock (Opțional):**
   - Apasă pe "⚙️ Setează Date Mock"
   - Introdu valori manual sau generează random
   - Salvează datele
4. **Sincronizează:**
   - Apasă "🔄 Sincronizează Acum"
   - Datele mock vor fi trimise la backend
   - Vei vedea statisticile actualizate

---

### 2. **Simulator iOS (Cu HealthKit Mock)**

**Requisit:**
- Xcode instalat
- iOS Simulator
- Device real pentru HealthKit (nu funcționează în simulator)

**Limitare:** HealthKit **NU funcționează în iOS Simulator**, doar pe device fizic.

**Alternativă:** Folosește modul MOCK în simulator.

---

### 3. **Android Emulator (Cu Google Fit Mock)**

**Requisit:**
- Android Studio
- Android Emulator cu Google Play Services

**Pași:**
1. Instalează Google Fit în emulator
2. Adaugă date de test în Google Fit
3. Aplicația va citi datele din Google Fit

**Alternativă:** Folosește modul MOCK (mai simplu).

---

### 4. **Device Fizic (Recomandat pentru Production)**

**iOS:**
- Device iPhone cu HealthKit
- Instalează aplicația prin Expo Go sau development build
- Acordă permisiuni HealthKit
- Datele reale vor fi sincronizate

**Android:**
- Device Android cu Google Play Services
- Instalează Google Fit (dacă nu este preinstalat)
- Acordă permisiuni
- Datele reale vor fi sincronizate

---

## 🎯 Workflow Recomandat pentru Testare

### Faza 1: Development (Fără Device)

1. **Folosește Modul MOCK:**
   ```
   - Aplicația detectează automat că nu ai device fizic
   - Activează modul MOCK automat
   - Generează date random pentru testare
   ```

2. **Setează Date Manual:**
   ```
   - Mergi în Profil → "Setează Date Mock"
   - Introdu valori custom pentru testare
   - Sau generează valori random
   ```

3. **Testează Sincronizarea:**
   ```
   - Apasă "Sincronizează Acum"
   - Verifică că datele ajung la backend
   - Verifică că statisticile se actualizează
   ```

### Faza 2: Testing (Cu Device - Opțional)

1. **Instalează Bibliotecile:**
   ```bash
   cd StepUp.Mobile
   npm install react-native-health
   ```

2. **Pentru iOS:**
   ```bash
   cd ios
   pod install
   ```

3. **Rulează pe Device:**
   ```bash
   npm run ios  # sau npm run android
   ```

4. **Acordă Permisiuni:**
   - iOS: Settings → Privacy → Health → StepUp
   - Android: Permisiuni automat la prima sincronizare

---

## 🔧 Configurare Modul MOCK

### Setare Date Mock Programatic

Poți seta date mock din cod pentru testare:

```javascript
import HealthService from './services/HealthService';

// Setează date pentru astăzi
await HealthService.setMockDataForDate(new Date(), {
  steps: 8500,
  calories: 350,
  distance: 6.5
});

// Apoi sincronizează
await HealthService.syncToday(userId);
```

### Date Mock Persistent

Datele mock sunt salvate în AsyncStorage și persistă între sesiuni:
- Cheie: `@StepUp:MockHealthData:steps:YYYY-MM-DD`
- Cheie: `@StepUp:MockHealthData:calories:YYYY-MM-DD`
- Cheie: `@StepUp:MockHealthData:distance:YYYY-MM-DD`

---

## 📊 Verificare Date în Backend

După sincronizare, verifică în backend:

1. **Endpoint:** `GET /api/activity` (dacă există)
2. **Database:** Tabela `ActivityLogs`
3. **Swagger:** `http://localhost:5205/swagger`

Datele ar trebui să aibă:
- `UserId` - ID-ul utilizatorului
- `Date` - Data pentru care sunt datele
- `MetricValue` - Valoarea (pași, calorii, etc.)

---

## 🐛 Debugging

### Verifică Modul MOCK

```javascript
import HealthService from './services/HealthService';

console.log('Mock Mode:', HealthService.isMockMode());
console.log('Has Permissions:', HealthService.hasHealthPermissions());
console.log('Initialized:', HealthService.isServiceInitialized());
```

### Verifică Date Mock Salvate

```javascript
import AsyncStorage from '@react-native-async-storage/async-storage';

const date = new Date().toISOString().split('T')[0];
const steps = await AsyncStorage.getItem(`@StepUp:MockHealthData:steps:${date}`);
console.log('Mock Steps:', steps);
```

### Log-uri în Console

HealthService loghează automat:
- `🏥 HealthService: Running in MOCK mode` - Modul MOCK activ
- `✅ HealthKit initialized` - HealthKit funcționează
- `✅ Google Fit authorized` - Google Fit funcționează

---

## ✅ Checklist Testare

- [ ] Modul MOCK se activează automat
- [ ] Poți seta date mock manual
- [ ] Sincronizarea funcționează cu date mock
- [ ] Datele ajung la backend
- [ ] Statisticile se actualizează în profil
- [ ] Datele mock persistă între sesiuni
- [ ] Poți sincroniza multiple zile

---

## 🚀 Următorii Pași

1. **Testează cu Modul MOCK** - Verifică că totul funcționează
2. **Testează pe Device Fizic** (opțional) - Când ai acces
3. **Implementează Auto-Sync** - Sincronizare automată în background
4. **Adaugă Grafic Progres** - Vizualizare date sincronizate

---

## 💡 Tips

- **Pentru Development:** Folosește întotdeauna modul MOCK
- **Pentru Testing:** Poți seta date specifice pentru scenarii de test
- **Pentru Production:** Instalează biblioteca și testează pe device real
- **Backend:** Backend-ul nu știe diferența între date mock și reale

---

## ❓ Probleme Comune

**Q: Nu văd butonul "Setează Date Mock"**
A: Verifică că rulezi în development mode și că modul MOCK este activ.

**Q: Datele nu se sincronizează**
A: Verifică că backend-ul rulează și că endpoint-ul `/api/health/sync` funcționează.

**Q: Vreau să testez cu date reale**
A: Instalează `react-native-health` și rulează pe device fizic.

---

**Gata de testare!** 🎉
Începe cu modul MOCK și testează funcționalitatea completă fără nevoie de device fizic.
