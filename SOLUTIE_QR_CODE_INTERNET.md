# 📱 Soluție: QR Code care Funcționează prin Internet

**Problema:** QR code-ul de la Expo funcționează doar pe același Wi-Fi.  
**Soluția:** Folosește **Expo Tunnel** pentru a face QR code-ul accesibil prin internet!

---

## 🚀 Soluția Rapidă

### Pentru TINE (cel care rulează backend-ul):

1. **Pornește backend-ul:**
   ```powershell
   cd StepUp.API
   dotnet run
   ```

2. **Pornește ngrok:**
   ```powershell
   ngrok http 5205
   ```
   Copiază URL-ul (ex: `https://abc123.ngrok-free.app`)

3. **Actualizează `HARDCODED_URL` în `network.js`:**
   ```javascript
   const HARDCODED_URL = 'abc123.ngrok-free.app';
   ```

4. **Pornește aplicația cu Tunnel:**
   ```powershell
   cd StepUp.Mobile
   npm run start:tunnel
   ```
   
   **SAU** rulează scriptul:
   ```powershell
   .\start-with-tunnel.ps1
   ```

5. **Partajează QR code-ul** cu prietenii (poți face screenshot sau share screen)

### Pentru PRIETENII:

1. **Instalează Expo Go** (gratis din App Store/Play Store)
2. **Deschide Expo Go**
3. **Scanează QR code-ul** pe care l-ai partajat
4. **Așteaptă** ca aplicația să se încarce
5. **GATA!** Aplicația funcționează! ✅

---

## 🔍 Ce este Expo Tunnel?

**Expo Tunnel** creează un tunel pentru Metro bundler (serverul de development), astfel încât:
- ✅ QR code-ul funcționează prin internet (nu doar pe Wi-Fi local)
- ✅ Prietenii pot scana QR code-ul de oriunde
- ✅ Nu trebuie să configureze nimic manual
- ✅ Funcționează automat cu Expo Go

---

## ⚙️ Diferența dintre Moduri

### `expo start` (Normal)
- ❌ QR code funcționează **doar pe același Wi-Fi**
- ✅ Mai rapid
- ✅ Nu necesită conexiune la serviciile Expo

### `expo start --tunnel` (Tunnel)
- ✅ QR code funcționează **prin internet**
- ✅ Prietenii pot scana de oriunde
- ⚠️ Puțin mai lent (trece prin serviciile Expo)
- ⚠️ Necesită cont Expo (gratis)

---

## 📋 Checklist Complet

### Pentru TINE:

- [ ] Backend-ul rulează (`dotnet run` în `StepUp.API`)
- [ ] ngrok rulează (`ngrok http 5205`)
- [ ] Am actualizat `HARDCODED_URL` cu URL-ul ngrok
- [ ] Am pornit aplicația cu `npm run start:tunnel`
- [ ] Am QR code-ul pregătit pentru partajare
- [ ] Am trimis QR code-ul prietenilor

### Pentru PRIETENII:

- [ ] Au instalat Expo Go pe telefon
- [ ] Au scanat QR code-ul
- [ ] Aplicația se încarcă
- [ ] Pot să se logheze

---

## 🎯 Workflow Zilnic

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
   Copiază URL-ul

3. **Actualizează `network.js`:**
   ```javascript
   const HARDCODED_URL = 'abc123.ngrok-free.app'; // URL-ul ngrok
   ```

4. **Pornește aplicația cu Tunnel:**
   ```powershell
   cd StepUp.Mobile
   npm run start:tunnel
   ```

5. **Partajează QR code-ul:**
   - Fă screenshot la QR code
   - Trimite-l prietenilor prin WhatsApp/Telegram/etc.
   - **SAU** share screen și lasă-i să scaneze

6. **Prietenii scanează QR code-ul** cu Expo Go și aplicația se încarcă!

---

## ❓ Probleme Comune

### "QR code nu se scanează"
- ✅ Verifică că ai folosit `npm run start:tunnel` (nu `npm start`)
- ✅ Verifică că ai conexiune la internet
- ✅ Verifică că Expo Go este instalat corect

### "Aplicația nu se încarcă după scanare"
- ✅ Verifică că backend-ul rulează
- ✅ Verifică că ngrok rulează
- ✅ Verifică că `HARDCODED_URL` este setat corect
- ✅ Verifică conexiunea la internet

### "Aplicația se încarcă dar nu se conectează la backend"
- ✅ Verifică că `HARDCODED_URL` este setat cu URL-ul ngrok corect
- ✅ Verifică că ngrok încă rulează (URL-ul nu s-a schimbat)
- ✅ Verifică că backend-ul răspunde la `http://localhost:5205/api`

---

## 💡 Tips

1. **Păstrează terminalele deschise:**
   - Terminal 1: Backend (`dotnet run`)
   - Terminal 2: ngrok (`ngrok http 5205`)
   - Terminal 3: Expo Tunnel (`npm run start:tunnel`)

2. **Partajează QR code-ul într-un grup:**
   - Creează un grup WhatsApp/Telegram
   - Trimite QR code-ul acolo
   - Toți pot scana același QR code

3. **Actualizează când ngrok se repornește:**
   - Dacă ngrok se repornește, URL-ul se schimbă
   - Actualizează `HARDCODED_URL` cu noul URL
   - Repornește aplicația (`npm run start:tunnel`)
   - Trimite noul QR code (dacă s-a schimbat)

---

## 🎉 Rezumat

**Pentru tine:**
1. Backend + ngrok + `HARDCODED_URL` configurat
2. `npm run start:tunnel`
3. Partajează QR code-ul

**Pentru prieteni:**
1. Instalează Expo Go
2. Scanează QR code-ul
3. GATA!

**Nu mai trebuie să configureze nimic manual!** 🎉

---

**Succes! 🚀**
