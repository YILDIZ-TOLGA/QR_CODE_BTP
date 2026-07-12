# BTPSecure — Brief projet (lis-moi en premier)

## Identité
- **Type** : Blazor WebAssembly Hosted (.NET 10) + PostgreSQL + Railway
- **Path local** : `C:\Users\y1903\Desktop\BTPSecure`
- **Repo** : `github.com/YILDIZ-TOLGA/QR_CODE_BTP` (branche `main`)
- **Prod** : https://qrcodebtp-production.up.railway.app
- **Domaine** : sécurisation d'achats BTP via QR codes (rôles : Admin / Dirigeant / Collaborateur / Fournisseur)

## Stack
- **Server** : ASP.NET Core, EF Core (Npgsql), JWT + BCrypt, QuestPDF
- **Client** : Blazor WASM + Radzen.Blazor (UI 100 % Radzen, **jamais** d'HTML/CSS/JS custom)
- **Shared** : DTOs / Entités / Enums partagés
- **Deploy** : Dockerfile copie `publish/` (pré-compilé) → Railway

## Arborescence clé
```
BTPSecure/
├── BTPSecure.Server/
│   ├── Controllers/   C_*.cs        (ex: C_Admin, C_Auth, C_Code)
│   ├── Services/      S_*.cs        (logique métier)
│   ├── DAO/           DAO_*.cs      (accès EF)
│   ├── Data/AppDbContext.cs
│   ├── Migrations/                   (PublishTrimmed=false obligatoire)
│   └── Program.cs                    (seed Admin + auto-migrate)
├── BTPSecure.Client/
│   ├── Pages/         Page_*.razor
│   ├── Components/    Comp_*.razor
│   ├── Services/      S_*.cs (appels HTTP)
│   ├── Layout/MainLayout.razor       (sidebar + auth subscription)
│   └── wwwroot/index.html            (loader stylisé)
├── BTPSecure.Shared/
│   ├── DTOs/          DTO_*.cs
│   ├── Entites/       E_*.cs
│   └── Enums/         Enum_*.cs
├── publish/                          (artefacts buildés, COMMITÉS pour Docker)
├── Dockerfile, railway.json, DEPLOIEMENT.md
```

## Conventions de nommage (STRICTES)
- **Préfixes fichiers** : `C_` controllers, `S_` services, `DAO_`, `DTO_`, `E_` entités, `Enum_`, `Page_`, `Comp_`
- **Variables locales** : `_camelCase` (ex: `_utilisateur`, `_claim`, `_dto`)
- **Paramètres méthodes** : `p_camelCase` (ex: `p_id`, `p_dto`, `p_entrepriseId`)
- **Propriétés publiques** : `PascalCase`
- **Tout en français** : `Connecter`, `Sauvegarder`, `Utilisateur`, `EstAutorisee`...

## Préférences code (RÈGLES UTILISATEUR)
- ❌ **JAMAIS** : `? :` (ternaire), `?.` (null-conditional), `??` (null-coalescing), switch expressions
- ✅ **TOUJOURS** : `if / else if / else` brutes, `if (x == null) { }` explicites
- ❌ Pas d'arguments nommés avec `:` → utiliser positionnel (ex: `NavigateTo(url, false, true)`)
- ❌ Pas d'HTML/CSS/JS dans les composants → **full Radzen** (RadzenCard, RadzenStack, RadzenFormField, etc.)
- ✅ Commentaires minimaux et en français
- ✅ Tout le texte UI en français

## Rôles & Auth
- `Enum_Role` : `Admin=0`, `Dirigeant=1`, `Collaborateur=2`, `Fournisseur=3`
- **Admin seed** : `admin@btpsecure.fr` / `Aqwxcvbn$74123-` (créé au démarrage si absent)
- **Flow** : login → `S_Auth.Connecter` → JWT en localStorage (clé `"token"`) → `S_AuthStateProvider` lit + set `HttpClient.Authorization`
- **Claims JWT** : `NameIdentifier` (Id), `Email`, `Role`
- **Redirections post-login par rôle** : `/admin`, `/dirigeant`, `/collaborateur`, `/fournisseur`
- `MainLayout` s'abonne à `AuthenticationStateChanged` pour MAJ live de la sidebar

## Workflow déploiement (CRITIQUE)
```bash
cd /c/Users/y1903/Desktop/BTPSecure

# 1) Publish
dotnet publish BTPSecure.Server/BTPSecure.Server.csproj -c Release -o publish

# 2) Vérifier le fingerprint Blazor
ls publish/wwwroot/_framework/ | grep '^blazor.webassembly\.[a-z0-9]*\.js$'
grep -o 'blazor.webassembly[^"]*' publish/wwwroot/index.html
# Si placeholder reste : sed -i 's|blazor.webassembly#\[.{fingerprint}\].js|blazor.webassembly.HASH.js|g' publish/wwwroot/index.html

# 3) Commit & push (publish/ doit être inclus)
git add <fichiers source> publish/
git commit -m "..."
git push   # Railway redéploie auto via webhook GitHub
```

## Pièges connus (NE PAS RÉINVENTER)
1. **IL Trimmer .NET 10** : `HttpClient.PostAsync` / `SendAsync` / `PutAsync` sont **trimmés** → erreur `Method not found`
   - **Solution** : utiliser `PostAsJsonAsync($"api/...", new { })` même pour des POST sans body
   - Côté serveur : changer `[HttpPut]` en `[HttpPost]` si besoin
2. **Migrations EF supprimées par le trimmer** : `<PublishTrimmed>false</PublishTrimmed>` dans `BTPSecure.Server.csproj`
3. **Fingerprint Blazor non résolu** : `index.html` peut garder `blazor.webassembly#[.{fingerprint}].js` après publish → toujours vérifier et `sed` si besoin
4. **Railway "Redeploy"** ≠ deploy du dernier commit. Si webhook raté → push commit vide :
   `git commit --allow-empty -m "trigger redeploy" && git push`
5. **AuthorizeView imbriqués** → conflit de `context`. Préférer un seul wrapper + variables `_estConnecte`/`_role`/`_email` lues via `OnInitializedAsync` + abonnement à `AuthenticationStateChanged`.
6. **DialogService.Confirm** trim-safe : éviter `range[..1]` ou string interpolation complexe ; pré-calculer les chaînes.
7. **Erreurs JSON vides côté client** : helper `LireMessageErreur` pour catcher les bodies vides/non-JSON.

## Endpoints diagnostiques
- `GET /health` → `200 ok` (utilisé par Railway healthcheck)
- `GET /db-status` → état connexion BDD
- `GET /env-keys` → clés env présentes

## Variables d'env (Railway)
- `DATABASE_URL` (Postgres connection string)
- `Jwt:Cle`, `Jwt:Emetteur`, `Jwt:Audience`, `Jwt:DureeHeures`
- `ASPNETCORE_ENVIRONMENT=Production`, `PORT` (auto-fourni)
- `Program.cs` réinjecte les env vars dans `IConfiguration` au boot

## Features récentes implémentées
- **Admin** : `EstAutorisee` sur `E_Entreprise` ; un Dirigeant ne peut générer de QR que si Admin a activé son entreprise
- **Multi-entreprises** : `E_CollaborateurEntreprise` (N-N, table physique `salaries_entreprises`) avec `Enum_StatutInvitation` (EnAttente / Acceptee / Refusee)
- **Invitations** : Dirigeant invite → Collaborateur accepte/refuse ; Collaborateur peut quitter (révoque ses codes)
- ⚠️ **Renommage Lot 0B (2026-07)** : `Patron`→`Dirigeant`, `Salarie`→`Collaborateur`, `Confiance`→`LibreService`, `E_SalarieEntreprise`→`E_CollaborateurEntreprise`. **Colonnes/tables physiques PostgreSQL inchangées** (`PatronId`, `SalarieId`, `salaries_entreprises`) via `HasColumnName`/`ToTable` dans `AppDbContext` — ne jamais toucher ces strings de mapping. Rôles JWT écrits par `.ToString()` → tokens émis avant le renommage exigent un re-login.
- **Sidebar conditionnelle** : cachée si non connecté ; menu burger caché aussi
- **Highlight exact** des items menu : `Match="NavLinkMatch.All"`
- **Loader index.html** stylisé (bouclier glassmorphism, dégradé bleu)
- **Persistance session** : `Page_Connexion.OnInitializedAsync` redirige si déjà authentifié

## Pattern services client (HTTP)
```csharp
// Toujours PostAsJsonAsync (jamais PostAsync/SendAsync/PutAsync)
var _reponse = await _http.PostAsJsonAsync($"api/admin/basculer-autorisation/{p_id}", new { });
if (!_reponse.IsSuccessStatusCode)
{
    var _msg = await LireMessageErreur(_reponse);
    return (false, _msg);
}
```

## Commandes utiles
```bash
git log --oneline -5
dotnet publish BTPSecure.Server/BTPSecure.Server.csproj -c Release -o publish
curl -s https://qrcodebtp-production.up.railway.app/health
curl -s https://qrcodebtp-production.up.railway.app/ | grep -o 'blazor.webassembly[^"]*'
git commit --allow-empty -m "trigger redeploy" && git push
```
