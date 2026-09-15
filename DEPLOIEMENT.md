# Deploiement BTPSecure sur Railway

> Marque publique : **KEYDO** — domaine **keydopro.com**. « BTPSecure » reste le nom technique du repo/projet.

## 1. Prerequis

- Un compte Railway (railway.com)
- Git installe sur ta machine
- Le projet pousse sur un repo GitHub

## 2. Deploiement en 5 minutes

### Etape 1 : Publier PUIS pousser sur GitHub

> ⚠️ **Le plus important de cette page.** Le `Dockerfile` fait `COPY publish/ .` : il **ne compile rien**, il copie le dossier `publish/` tel qu'il est dans le repo. Pousser du code source sans republier deploie donc **l'ancienne version** — le build Railway reussit, le site ne change pas, et on cherche le probleme du mauvais cote.

```bash
cd /c/Users/y1903/Desktop/BTPSecure

# 1) Compiler l'artefact qui sera reellement deploye
dotnet publish BTPSecure.Server/BTPSecure.Server.csproj -c Release -o publish

# 2) Verifier le fingerprint Blazor (piege recurrent, voir plus bas)
ls publish/wwwroot/_framework/ | grep '^blazor.webassembly\.[a-z0-9]*\.js$'
grep -o 'blazor.webassembly[^"]*' publish/wwwroot/index.html

# 3) Commiter les sources ET publish/
git add <fichiers source> publish/
git commit -m "Description du changement"
git push        # Railway redeploie automatiquement via le webhook GitHub
```

Le repo est deja initialise : `github.com/YILDIZ-TOLGA/QR_CODE_BTP`, branche `main`.

#### Le fingerprint Blazor (a verifier a chaque publish touchant `index.html`)

Blazor ecrit dans `index.html` un nom de fichier horodate, par exemple `blazor.webassembly.66stpp682q.js`. Il arrive que le publish laisse le **placeholder** non resolu :

```
blazor.webassembly#[.{fingerprint}].js
```

Dans ce cas le navigateur demande un fichier inexistant et **l'application reste bloquee sur le loader**. Correctif :

```bash
sed -i 's|blazor.webassembly#\[\.{fingerprint}\]\.js|blazor.webassembly.LE_VRAI_HASH.js|g' publish/wwwroot/index.html
```

Les deux commandes de l'etape 2 ci-dessus doivent afficher **le meme nom de fichier**.

### Etape 2 : Creer le projet sur Railway

1. Va sur **railway.com** > **New Project**
2. Clique sur **Deploy from GitHub repo**
3. Selectionne le repo `QR_CODE_BTP`
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
| `SMTP_FROM` | `contact@keydopro.com` (domaine authentifie chez Brevo) |
| `SMTP_FROM_NAME` | `KEYDO` |
| `SITE_URL` | `https://www.keydopro.com` (liens dans les emails) |
| `ADMIN_EMAIL` | `admin_acc@keydopro.com` (login du compte admin auto-cree) |
| `ADMIN_PASSWORD` | Mot de passe du compte admin (**obligatoire** ; sans lui, aucun admin n'est cree) |
| `SESSION_UNIQUE_EXCLUSIONS` | *(facultative)* Roles exemptes de la session unique, separes par des virgules (ex. `Fournisseur`). **Absente = tous les roles concernes.** |

> `DATABASE_URL` et `PORT` sont injectes automatiquement par Railway.
> ⚠️ `ADMIN_PASSWORD` ne doit **jamais** etre en dur dans le code (le seed le lit depuis l'env).

**Emails (Brevo) :** on utilise l'**API HTTP** de Brevo (`api.brevo.com/v3/smtp/email`), pas le SMTP — Railway bloque les ports SMTP sortants. Brevo exige que l'**IP de sortie Railway** soit whitelistee (ou desactive la restriction IP dans les parametres Brevo). Quota gratuit : 300 emails/jour.

### Domaine personnalise (OVH → Railway)

1. Service Railway > **Settings** > **Networking** > **Custom Domain** > ajoute `www.keydopro.com`
2. Chez OVH : un enregistrement **CNAME** `www` vers la cible fournie par Railway (⚠️ supprimer d'abord les A/AAAA `www` par defaut, sinon erreur OVH « CNAME and other data »)
3. Apex (`keydopro.com`) : **redirection** vers `www` (les CNAME sur l'apex sont interdits). Ne pas laisser l'assistant OVH rediriger **aussi** `www` — ca ecraserait le CNAME.
4. Verification TXT si demandee : enregistrement `_railway-verify.www`

### Etape 5 : Generer un domaine

1. Clique sur ton service > **Settings** > **Networking**
2. Clique **Generate Domain** pour obtenir une URL publique `https://btpsecure-xxx.up.railway.app`

## 3. C'est pret !

- Les migrations s'executent automatiquement au demarrage
- Le Dockerfile installe les dependances pour la generation PDF (QuestPDF)
- Railway reconstruit l'image a chaque `git push` sur `main`… **mais ne compile pas le .NET** : il copie le `publish/` du repo (voir Etape 1)
- « Redeploy » dans Railway **ne redeploie pas le dernier commit**. Si le webhook a ete rate : `git commit --allow-empty -m "trigger redeploy" && git push`

## 4. Verifier les logs

Dans Railway, clique sur ton service puis sur l'onglet **Logs** pour voir les logs en temps reel.

### Un email n'arrive pas ?

Les envois sont traces dans les logs Railway. Cherche `Brevo` :

| Symptome dans les logs | Cause | Correctif |
|---|---|---|
| `BREVO_API_KEY manquante` | Variable absente | Ajouter `BREVO_API_KEY` dans Railway |
| `Erreur Brevo ... 401 ... unrecognised IP address` | IP de sortie Railway non autorisee | Whitelister l'IP chez Brevo, ou desactiver la restriction IP |
| `Erreur Brevo ... 402` / quota | **300 emails/jour** (offre gratuite) depasse | Attendre le lendemain ou passer a une offre payante |
| Rien du tout | L'action n'envoie pas d'email | Verifier le parcours concerne |

L'app envoie un email pour : creation de compte, invitation de collaborateur, code envoye a un tiers, invitation fournisseur, verification d'email, reinitialisation de mot de passe, message a un destinataire externe. Le quota gratuit part vite.

> A la **creation d'un collaborateur**, si l'email echoue, le **mot de passe temporaire est affiche au createur** (il n'existe que dans cet email) : transmets-le manuellement, sinon le compte est inutilisable.

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
| `SMTP_FROM` | Manuelle | Email expediteur (`contact@keydopro.com`, domaine authentifie chez Brevo) |
| `SMTP_FROM_NAME` | Manuelle | Nom affiche de l'expediteur (`KEYDO`) |
| `SITE_URL` | Manuelle | URL publique du site (`https://www.keydopro.com`, liens dans les emails) |
| `ADMIN_EMAIL` | Manuelle | Login du compte admin seede au demarrage |
| `ADMIN_PASSWORD` | Manuelle | Mot de passe du compte admin (sans lui, aucun admin cree) |
| `SESSION_UNIQUE_EXCLUSIONS` | Manuelle, facultative | Roles exemptes de la session unique. Lue **au demarrage** : la changer exige un redemarrage du service. |

## 6. Tester en local avant de pousser

⚠️ **Il n'y a pas de PostgreSQL sur la machine de dev** (ni Docker, ni `psql`, ni service installe). Tout ce qui touche la base se valide donc **en prod**. Ce qu'on peut verifier en local reste utile : la compilation, le demarrage, l'injection de dependances, les 401 sur les endpoints proteges et le contenu du WASM publie.

Le plus fiable est de lancer **l'artefact reellement deploye** (donc trimme, comme en prod) plutot que `dotnet run` :

```bash
cd /c/Users/y1903/Desktop/BTPSecure/publish
ASPNETCORE_ENVIRONMENT=Production dotnet BTPSecure.Server.dll --urls http://localhost:5199
```

Les erreurs Npgsql au demarrage sont **normales** (aucune base joignable) : l'app sert quand meme le WASM et les endpoints qui n'ont pas besoin de la base.

Puis **arreter le serveur avant tout nouveau publish**, sinon la DLL reste verrouillee et le publish echoue en `MSB3027` :

```powershell
Get-NetTCPConnection -LocalPort 5199 -State Listen | Select-Object -First 1 -ExpandProperty OwningProcess | Stop-Process -Force
```

Avec une base PostgreSQL disponible, le parcours classique reste :

```bash
dotnet ef database update --project BTPSecure.Server
dotnet run --project BTPSecure.Server   # http://localhost:5137
```
