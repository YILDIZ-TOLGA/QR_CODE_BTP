# Deploiement BTPSecure sur Railway

## 1. Prerequis

- Un compte Railway (railway.com)
- Git installe sur ta machine
- Le projet pousse sur un repo GitHub

## 2. Deploiement en 5 minutes

### Etape 1 : Pousser le code sur GitHub

```bash
cd BTPSecure
git init
git add .
git commit -m "Initial commit - BTPSecure"
git remote add origin https://github.com/TON_USER/btpsecure.git
git push -u origin main
```

### Etape 2 : Creer le projet sur Railway

1. Va sur **railway.com** > **New Project**
2. Clique sur **Deploy from GitHub repo**
3. Selectionne ton repo `btpsecure`
4. Railway detecte automatiquement le `Dockerfile` et lance le build

### Etape 3 : Ajouter PostgreSQL

1. Dans ton projet Railway, clique **+ New** > **Database** > **Add PostgreSQL**
2. Railway cree une base PostgreSQL et injecte automatiquement la variable `DATABASE_URL`
3. Lie la base au service : clique sur ton service > **Variables** > **Add Reference Variable** > selectionne `DATABASE_URL` depuis PostgreSQL

### Etape 4 : Configurer les variables d'environnement

Dans ton service Railway, va dans l'onglet **Variables** et ajoute :

| Variable | Valeur |
|----------|--------|
| `JWT_CLE` | `UneCleSuperSecreteDe32CaracteresMinimum!` |
| `JWT_EMETTEUR` | `BTPSecure` |
| `JWT_AUDIENCE` | `BTPSecure` |
| `JWT_DUREE_HEURES` | `24` |
| `BREVO_API_KEY` | Cle API Brevo (envoi des emails) |
| `SMTP_FROM` | `contact@codebtpsecure.cloud` (email expediteur verifie chez Brevo) |
| `SMTP_FROM_NAME` | `BTPSecure` |
| `SITE_URL` | `https://www.codebtpsecure.cloud` (liens dans les emails) |

> `DATABASE_URL` et `PORT` sont injectes automatiquement par Railway.

**Emails (Brevo) :** on utilise l'**API HTTP** de Brevo (`api.brevo.com/v3/smtp/email`), pas le SMTP — Railway bloque les ports SMTP sortants. Brevo exige que l'**IP de sortie Railway** soit whitelistee (ou desactive la restriction IP dans les parametres Brevo). Quota gratuit : 300 emails/jour.

### Domaine personnalise (OVH → Railway)

1. Service Railway > **Settings** > **Networking** > **Custom Domain** > ajoute `www.codebtpsecure.cloud`
2. Chez OVH : un enregistrement **CNAME** `www` vers la cible fournie par Railway
3. Apex (`codebtpsecure.cloud`) : redirection vers `www` (les CNAME sur l'apex sont interdits)
4. Verification TXT si demandee : enregistrement `_railway-verify.www`

### Etape 5 : Generer un domaine

1. Clique sur ton service > **Settings** > **Networking**
2. Clique **Generate Domain** pour obtenir une URL publique `https://btpsecure-xxx.up.railway.app`

## 3. C'est pret !

- Les migrations s'executent automatiquement au demarrage
- Le Dockerfile installe les dependances pour la generation PDF (QuestPDF)
- Railway rebuild automatiquement a chaque `git push` sur `main`

## 4. Verifier les logs

Dans Railway, clique sur ton service puis sur l'onglet **Logs** pour voir les logs en temps reel.

## 5. Variables d'environnement (resume)

| Variable | Source | Description |
|----------|--------|-------------|
| `DATABASE_URL` | Auto (PostgreSQL Railway) | Connection string PostgreSQL |
| `PORT` | Auto (Railway) | Port HTTP du conteneur |
| `JWT_CLE` | Manuelle | Cle secrete JWT (min 32 chars) |
| `JWT_EMETTEUR` | Manuelle | Emetteur du token JWT |
| `JWT_AUDIENCE` | Manuelle | Audience du token JWT |
| `JWT_DUREE_HEURES` | Manuelle | Duree de validite du token (heures) |
| `BREVO_API_KEY` | Manuelle | Cle API Brevo pour l'envoi des emails |
| `SMTP_FROM` | Manuelle | Email expediteur verifie chez Brevo |
| `SMTP_FROM_NAME` | Manuelle | Nom affiche de l'expediteur |
| `SITE_URL` | Manuelle | URL publique du site (liens dans les emails) |

## 6. Developpement local

Pour tester en local, utilise `appsettings.json` avec une base PostgreSQL locale :

```bash
# Installer PostgreSQL localement, puis :
dotnet ef database update --project BTPSecure.Server
dotnet run --project BTPSecure.Server
```

L'app est accessible sur `http://localhost:5137`.
