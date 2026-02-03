# Ghid: Punerea Proiectului StepUp pe GitHub

## 📋 Pași de Urmat

### 1. Verifică că Git este instalat
```powershell
git --version
```
Dacă nu este instalat, descarcă de la: https://git-scm.com/download/win

### 2. Inițializează Repository-ul Git

```powershell
# Asigură-te că ești în folderul proiectului
cd "C:\Users\User\Desktop\desktop 20256\StepUp"

# Inițializează repo-ul Git
git init

# Adaugă toate fișierele (respectând .gitignore)
git add .

# Faci primul commit
git commit -m "Initial commit: StepUp project"
```

### 3. Creează Repository pe GitHub

1. **Deschide GitHub:** https://github.com
2. **Loghează-te** în contul tău
3. **Creează un repository nou:**
   - Click pe **"+"** din colțul dreapta sus → **"New repository"**
   - Nume: `StepUp` (sau alt nume preferat)
   - Descriere: `StepUp - Mobile fitness challenge application`
   - **Public** sau **Private** (alege)
   - **NU** bifezi "Add a README file" (ai deja unul)
   - **NU** bifezi "Add .gitignore" (ai deja unul)
   - **NU** bifezi "Choose a license"
   - Click **"Create repository"**

### 4. Conectează Repository-ul Local cu GitHub

După ce ai creat repository-ul pe GitHub, vei vedea instrucțiuni. Folosește:

```powershell
# Adaugă remote-ul GitHub (înlocuiește USERNAME cu username-ul tău)
git remote add origin https://github.com/USERNAME/StepUp.git

# Verifică că remote-ul este adăugat corect
git remote -v
```

### 5. Faci Push pe GitHub

```powershell
# Schimbă numele branch-ului principal în "main" (dacă nu este deja)
git branch -M main

# Faci push pe GitHub
git push -u origin main
```

**Notă:** Dacă ești logat în GitHub Desktop sau ai configurat credentialele Git, va funcționa direct. Altfel, GitHub va cere autentificare.

### 6. Autentificare GitHub (dacă este necesar)

Dacă Git cere autentificare:
- **Personal Access Token:** GitHub nu mai acceptă parole simple
- Creează un token: https://github.com/settings/tokens
  - Click **"Generate new token"** → **"Generate new token (classic)"**
  - Nume: `StepUp Project`
  - Bifează `repo` (toate permisiunile repo)
  - Click **"Generate token"**
  - **Copiază token-ul** (nu îl vei mai vedea!)
- Când Git cere parolă, folosește token-ul în loc de parolă

## 🔒 Securitate - Fișiere Sensibile

**IMPORTANT:** Am configurat `.gitignore` să excludă:
- `appsettings.json` - conține parole baza de date
- `appsettings.Development.json` - conține configurații de dezvoltare
- `uploads/` - conține imagini încărcate de utilizatori

### Pentru a rula proiectul după clone:

1. **Creează `appsettings.json` manual:**
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "YOUR_DATABASE_CONNECTION_STRING"
     },
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning"
       }
     },
     "AllowedHosts": "*",
     "Email": {
       "SmtpHost": "smtp.gmail.com",
       "SmtpPort": 587,
       "SmtpUsername": "YOUR_EMAIL",
       "SmtpPassword": "YOUR_APP_PASSWORD",
       "FromEmail": "YOUR_EMAIL",
       "FromName": "StepUp"
     },
     "AppSettings": {
       "BaseUrl": "YOUR_BASE_URL"
     }
   }
   ```

2. **Sau folosește User Secrets (recomandat):**
   ```powershell
   cd StepUp.API
   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "YOUR_CONNECTION_STRING"
   dotnet user-secrets set "Email:SmtpPassword" "YOUR_PASSWORD"
   ```

## 📝 Comenzi Rapide (Copy-Paste)

```powershell
# 1. Inițializează Git
git init

# 2. Adaugă fișierele
git add .

# 3. Faci commit
git commit -m "Initial commit: StepUp project"

# 4. Adaugă remote (înlocuiește USERNAME)
git remote add origin https://github.com/USERNAME/StepUp.git

# 5. Schimbă branch în main
git branch -M main

# 6. Push pe GitHub
git push -u origin main
```

## 🔄 Actualizări Viitoare

Când faci modificări și vrei să le pui pe GitHub:

```powershell
# Vezi ce s-a schimbat
git status

# Adaugă modificările
git add .

# Faci commit cu un mesaj descriptiv
git commit -m "Descrierea modificărilor"

# Faci push
git push
```

## 📚 Resurse Utile

- **Git Documentation:** https://git-scm.com/doc
- **GitHub Guides:** https://guides.github.com
- **Git Cheat Sheet:** https://education.github.com/git-cheat-sheet-education.pdf

## ⚠️ Notă Importantă

Dacă ai deja un repository Git în altă parte sau vrei să folosești SSH în loc de HTTPS:

**Pentru SSH:**
```powershell
git remote add origin git@github.com:USERNAME/StepUp.git
```

**Pentru a schimba remote-ul:**
```powershell
git remote set-url origin https://github.com/USERNAME/StepUp.git
```
