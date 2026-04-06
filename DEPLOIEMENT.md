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

> `DATABASE_URL` et `PORT` sont injectes automatiquement par Railway.

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

## 6. Developpement local

Pour tester en local, utilise `appsettings.json` avec une base PostgreSQL locale :

```bash
# Installer PostgreSQL localement, puis :
dotnet ef database update --project BTPSecure.Server
dotnet run --project BTPSecure.Server
```

L'app est accessible sur `http://localhost:5137`.
