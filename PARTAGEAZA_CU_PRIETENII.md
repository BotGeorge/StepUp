# 📤 Ghid: Cum să partajezi aplicația cu prietenii

Acest ghid este pentru **tine** (cel care rulează backend-ul) - cum să le dai prietenilor acces la aplicație.

---

## 🎯 Situația Actuală

✅ Ai backend-ul pornit  
✅ Ai ngrok configurat  
✅ URL-ul funcționează pe telefonul tău  
❓ **Ce trebuie să facă prietenii?**

---

## 📋 Răspunsul Scurt

**DA, prietenii trebuie să aibă aplicația!** Dar există mai multe moduri:

### Opțiunea 1: Expo Go + QR Code (CEL MAI SIMPLU) ⭐
- Prietenii instalează **Expo Go** (gratis din App Store/Play Store)
- Tu pornești aplicația mobile (`npm start`)
- Ei scanează **QR code-ul** cu Expo Go
- **GATA!** Aplicația se încarcă automat

### Opțiunea 2: Clonare Repository (Dacă au acces la cod)
- Prietenii clonează repository-ul
- Instalează dependențele (`npm install`)
- Configurează `HARDCODED_URL` cu URL-ul tău ngrok
- Pornesc aplicația (`npm start`)

### Opțiunea 3: Build Aplicație (Pentru distribuție)
- Creezi un build al aplicației (APK/IPA)
- Distribui build-ul prietenilor
- Ei instalează direct pe telefon

---

## 🚀 SOLUȚIA RECOMANDATĂ: Expo Go + QR Code

### Pașii pentru TINE:

1. **Pornește aplicația mobile:**
   ```powershell
   cd StepUp.Mobile
   npm start
   ```

2. **Vei vedea un QR code** în terminal sau browser

3. **Spune-le prietenilor:**
   - "Instalați Expo Go din App Store/Play Store"
   - "Scanați acest QR code cu Expo Go"
   - "Aplicația se va încărca automat!"

### Pașii pentru PRIETENI:

1. **Instalează Expo Go** (gratis)
2. **Deschide Expo Go**
3. **Scanează QR code-ul** pe care l-ai partajat
4. **Așteaptă** ca aplicația să se încarce
5. **GATA!** Pot folosi aplicația

**⚠️ IMPORTANT:** Pentru ca QR code-ul să funcționeze, prietenii trebuie să fie pe **același internet** cu tine SAU să folosești **ngrok tunnel** pentru Metro bundler (Expo).

---

## 🔧 SOLUȚIA ALTERNATIVĂ: Configurare Manuală

Dacă prietenii au acces la codul sursă:

### 1. Partajează-le URL-ul ngrok

Trimite-le mesaj:
```
URL ngrok: https://abc123.ngrok-free.app

Configurați în StepUp.Mobile/config/network.js:
const HARDCODED_URL = 'abc123.ngrok-free.app';
```

### 2. Ei clonează repository-ul

```bash
git clone [URL_REPOSITORY]
cd StepUp/StepUp.Mobile
npm install
```

### 3. Ei configurează URL-ul

Deschid `StepUp.Mobile/config/network.js` și actualizează:
```javascript
const HARDCODED_URL = 'abc123.ngrok-free.app'; // URL-ul tău
```

### 4. Ei pornesc aplicația

```bash
npm start
```

Apoi scanează QR code-ul cu Expo Go.

---

## 📱 SOLUȚIA AVANSATĂ: Build Aplicație

Dacă vrei să distribui aplicația fără Expo Go:

### Pentru Android (APK):

1. **Configurează EAS Build:**
   ```bash
   npm install -g eas-cli
   eas login
   eas build:configure
   ```

2. **Creează build:**
   ```bash
   eas build --platform android --profile preview
   ```

3. **Distribuie APK-ul** prietenilor

### Pentru iOS (TestFlight):

1. **Configurează EAS Build:**
   ```bash
   eas build:configure
   ```

2. **Creează build:**
   ```bash
   eas build --platform ios --profile preview
   ```

3. **Distribuie prin TestFlight**

**⚠️ NOTĂ:** Build-urile necesită cont Apple Developer (plătit) pentru iOS.

---

## 🔄 Workflow Zilnic

### Când vrei să permiți prietenilor să se conecteze:

1. **Pornește backend-ul:**
   ```powershell
   cd StepUp.API
   dotnet run
   ```

2. **Pornește ngrok:**
   ```powershell
   ngrok http 5205
   ```

3. **Copiază URL-ul ngrok** (ex: `https://abc123.ngrok-free.app`)

4. **Actualizează `HARDCODED_URL`** în `network.js`:
   ```javascript
   const HARDCODED_URL = 'abc123.ngrok-free.app';
   ```

5. **Pornește aplicația mobile:**
   ```powershell
   cd StepUp.Mobile
   npm start
   ```

6. **Partajează cu prietenii:**
   - **QR code-ul** (dacă folosesc Expo Go)
   - **SAU** URL-ul ngrok (dacă au codul sursă)

### Când ngrok se repornește:

1. **Obține noul URL** din terminalul ngrok
2. **Actualizează `HARDCODED_URL`** în `network.js`
3. **Repornește aplicația mobile**
4. **Trimite noul URL** prietenilor (dacă au codul sursă)

---

## 💬 Mesaj Template pentru Prieteni

```
Salut! Vrei să testezi aplicația StepUp?

Opțiunea 1 (Simplă):
1. Instalează "Expo Go" din App Store/Play Store
2. Scanează acest QR code: [ATAȘEAZĂ QR CODE]
3. Aplicația se va încărca automat!

Opțiunea 2 (Dacă ai codul):
1. Clonează repository-ul
2. Configurează HARDCODED_URL = 'abc123.ngrok-free.app'
3. Rulează npm start și scanează QR code-ul

URL ngrok: https://abc123.ngrok-free.app
```

---

## ✅ Checklist pentru Tine

- [ ] Backend-ul rulează (`dotnet run` în `StepUp.API`)
- [ ] ngrok rulează (`ngrok http 5205`)
- [ ] Am actualizat `HARDCODED_URL` cu URL-ul ngrok
- [ ] Aplicația mobile rulează (`npm start` în `StepUp.Mobile`)
- [ ] Am QR code-ul pregătit pentru partajare
- [ ] Am trimis instrucțiuni prietenilor
- [ ] Am partajat URL-ul ngrok (dacă au codul sursă)

---

## 🎯 Rezumat

**Prietenii trebuie să:**
1. ✅ Aibă aplicația (Expo Go SAU codul sursă)
2. ✅ Configureze URL-ul ngrok (dacă au codul sursă)
3. ✅ Se conecteze la același backend prin ngrok

**Tu trebuie să:**
1. ✅ Rulezi backend-ul
2. ✅ Rulezi ngrok
3. ✅ Partajezi QR code-ul SAU URL-ul ngrok
4. ✅ Anunți prietenii când URL-ul se schimbă

---

**Succes! 🎉**
