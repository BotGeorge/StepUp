# 📧 Configurare SMTP pentru Verificare Email

Acest ghid te ajută să configurezi SMTP-ul pentru trimiterea email-urilor de verificare.

## 🔧 Configurare în `appsettings.json`

Deschide `StepUp.API/appsettings.json` și actualizează secțiunea `Email`:

```json
{
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "FromEmail": "your-email@gmail.com",
    "FromName": "StepUp"
  },
  "AppSettings": {
    "BaseUrl": "http://localhost:5205"
  }
}
```

## 📮 Configurare Gmail (Recomandat)

### Pasul 1: Activează autentificarea în 2 pași

1. Mergi la https://myaccount.google.com/security
2. Activează "Verificare în doi pași" (2-Step Verification)

### Pasul 2: Generează o parolă de aplicație

1. Mergi la https://myaccount.google.com/apppasswords
2. Selectează "Mail" ca aplicație
3. Selectează "Other (Custom name)" ca dispozitiv
4. Introdu "StepUp" ca nume
5. Click pe "Generate"
6. Copiază parola generată (16 caractere, fără spații)

### Pasul 3: Configurează în `appsettings.json`

```json
{
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "xxxx xxxx xxxx xxxx",  // Parola de aplicație generată
    "FromEmail": "your-email@gmail.com",
    "FromName": "StepUp"
  }
}
```

**⚠️ IMPORTANT:** Folosește parola de aplicație, NU parola contului tău Gmail!

## 📧 Configurare Outlook/Hotmail

```json
{
  "Email": {
    "SmtpHost": "smtp-mail.outlook.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@outlook.com",
    "SmtpPassword": "your-password",
    "FromEmail": "your-email@outlook.com",
    "FromName": "StepUp"
  }
}
```

## 📧 Configurare Yahoo Mail

```json
{
  "Email": {
    "SmtpHost": "smtp.mail.yahoo.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@yahoo.com",
    "SmtpPassword": "your-app-password",
    "FromEmail": "your-email@yahoo.com",
    "FromName": "StepUp"
  }
}
```

**Notă:** Yahoo necesită și el o parolă de aplicație. Generează una din https://login.yahoo.com/account/security

## 🌐 Configurare pentru producție

### Actualizează `AppSettings:BaseUrl`

În `appsettings.json`, actualizează `BaseUrl` cu URL-ul real al aplicației:

```json
{
  "AppSettings": {
    "BaseUrl": "https://your-domain.com"  // Sau IP-ul public dacă folosești IP direct
  }
}
```

**Exemplu pentru Railway/Render:**
```json
{
  "AppSettings": {
    "BaseUrl": "https://stepup-api.railway.app"
  }
}
```

## 🔒 Securitate (Recomandat pentru producție)

Pentru producție, folosește variabile de mediu în loc să pui parola direct în `appsettings.json`:

### Variabile de mediu

```bash
Email__SmtpHost=smtp.gmail.com
Email__SmtpPort=587
Email__SmtpUsername=your-email@gmail.com
Email__SmtpPassword=your-app-password
Email__FromEmail=your-email@gmail.com
Email__FromName=StepUp
AppSettings__BaseUrl=https://your-domain.com
```

### Sau în `appsettings.Production.json`

Creează `appsettings.Production.json`:

```json
{
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "",
    "SmtpPassword": "",
    "FromEmail": "",
    "FromName": "StepUp"
  },
  "AppSettings": {
    "BaseUrl": ""
  }
}
```

Și setează valorile prin variabile de mediu sau Azure Key Vault / AWS Secrets Manager.

## ✅ Testare

După configurare, testează trimiterea email-ului:

1. Rulează aplicația backend
2. Încearcă să te înregistrezi cu un email real
3. Verifică inbox-ul (și spam-ul) pentru email-ul de verificare

## 🐛 Depanare

### Email-ul nu se trimite

1. **Verifică log-urile backend-ului** - ar trebui să vezi erori dacă există probleme
2. **Verifică parola de aplicație** - asigură-te că ai folosit parola corectă
3. **Verifică firewall-ul** - portul 587 trebuie să fie deschis
4. **Verifică că email-ul nu este în spam**

### Eroare: "Authentication failed"

- Verifică că ai folosit parola de aplicație, nu parola contului
- Pentru Gmail, asigură-te că ai activat "Verificare în doi pași"
- Verifică că username-ul este corect

### Eroare: "Connection timeout"

- Verifică că `SmtpHost` este corect
- Verifică că portul este corect (587 pentru TLS)
- Verifică firewall-ul și conexiunea la internet

## 📝 Notă importantă

- **NU** pune parola contului direct în cod pentru producție
- Folosește întotdeauna parolă de aplicație pentru Gmail/Yahoo
- Pentru producție, folosește variabile de mediu sau servicii de management al secretelor
