# BTPSecure — Brief projet (lis-moi en premier)

## Identité
- **Type** : Blazor WebAssembly Hosted (.NET 10) + PostgreSQL + Railway
- **Path local** : `C:\Users\y1903\Desktop\BTPSecure`
- **Repo** : `github.com/YILDIZ-TOLGA/QR_CODE_BTP` (branche `main`)
- **Prod** : https://qrcodebtp-production.up.railway.app
- **Domaine perso** : https://www.codebtpsecure.cloud (OVH → Railway, CNAME www + redirection apex)
- **Métier** : sécurisation d'achats BTP via QR codes (rôles : Admin / Dirigeant / Collaborateur / Fournisseur)

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
8. **Messagerie pièces jointes (dérogation assumée à la règle full Radzen — NE PAS « corriger »)** : `<InputFile>` (composant framework) pour lire les **octets** d'un fichier côté WASM (aucun composant Radzen ne le permet sans URL d'upload portant le JWT), et helper JS `window.btpTelechargerFichier` dans `index.html` pour télécharger (Chrome bloque la navigation vers les `data:` URI). `index.html` contient déjà du CSS/JS (loader) → c'est de l'infra, pas un composant.
9. **Listes de tickets : jamais charger le `bytea`** → projeter sur `TicketApercu` (présence de PJ déduite du nom). Les octets ne sont lus que par `ObtenirParId` pour le **téléchargement**. Marquage lu / purge via `ExecuteUpdate` / `ExecuteDelete` (pas de chargement d'entités).

## Endpoints diagnostiques
- `GET /health` → `200 ok` (utilisé par Railway healthcheck)
- `GET /db-status` → état connexion BDD
- `GET /env-keys` → clés env présentes

## Variables d'env (Railway)
- `DATABASE_URL` (Postgres connection string)
- `JWT_CLE`, `JWT_EMETTEUR`, `JWT_AUDIENCE`, `JWT_DUREE_HEURES`
- **Emails Brevo (API HTTP, pas SMTP — Railway bloque le SMTP)** : `BREVO_API_KEY`, `SMTP_FROM` (contact@codebtpsecure.cloud), `SMTP_FROM_NAME`, `SITE_URL`
- `ASPNETCORE_ENVIRONMENT=Production`, `PORT` (auto-fourni)
- `Program.cs` réinjecte les env vars dans `IConfiguration` au boot
- ⚠️ Brevo exige que l'IP de sortie Railway soit whitelistée (ou désactiver la restriction IP côté Brevo)

## Features récentes implémentées
- **Admin** : `EstAutorisee` sur `E_Entreprise` ; un Dirigeant ne peut générer de QR que si Admin a activé son entreprise
- **Multi-entreprises** : `E_CollaborateurEntreprise` (N-N, table physique `salaries_entreprises`) avec `Enum_StatutInvitation` (EnAttente / Acceptee / Refusee)
- **Invitations** : Dirigeant invite → Collaborateur accepte/refuse ; Collaborateur peut quitter (révoque ses codes)
- ⚠️ **Renommage Lot 0B (2026-07)** : `Patron`→`Dirigeant`, `Salarie`→`Collaborateur`, `Confiance`→`LibreService`, `E_SalarieEntreprise`→`E_CollaborateurEntreprise`. **Colonnes/tables physiques PostgreSQL inchangées** (`PatronId`, `SalarieId`, `salaries_entreprises`) via `HasColumnName`/`ToTable` dans `AppDbContext` — ne jamais toucher ces strings de mapping. Rôles JWT écrits par `.ToString()` → tokens émis avant le renommage exigent un re-login.
- **Rôles internes entreprise (Lot 1)** : `Enum_RoleEntreprise` {Collaborateur=1, Responsable=2, ResponsableAdmin=3} sur `E_CollaborateurEntreprise`. Responsable/RA = code permanent libre-service (`E_Code.EstPermanent`, régénéré à chaque validation, sans historique). Le RA peut créer des codes + a un tableau de bord (`Page_DashboardDirigeant`) mais **ne peut pas** révoquer son propre code ni celui d'un autre RA, ni changer les rôles.
- **Inscription 2 étapes (Lot 2)** : cartes Dirigeant / Collaborateur / Fournisseur. Création de collaborateur par le Dirigeant (email obligatoire, mot de passe temporaire envoyé par mail).
- **Logique codes (Lot 3)** : libre-service = **usage unique** ; validité **24 h fixe** ; type Liste avec achats supplémentaires **0/50/100/200 € HT** ; code pour un **tiers externe** (`E_Code.EmailTiers`, envoyé par mail). Type par défaut = Liste, case à cocher pour passer en Libre-service.
- **Espace fournisseur (Lot 4)** : validation admin des fournisseurs (`E_Utilisateur.EstValide`) ; **sous-comptes** (`ParentFournisseurId`, SIRET partagé) ; **blacklist** par email (`E_Blacklist`) ; navigation Accueil / À préparer / Prêtes ; notification « commande prête ».
- **Messagerie / tickets (Lot 5)** : `E_Ticket` (pièce jointe en `bytea`, **TTL 24 h** via `S_NettoyageTickets` BackgroundService) ; **annuaire** selon l'écosystème ; destinataire interne OU email externe (Brevo) ; **badge non-lus** sidebar. `Page_Messagerie` : vues Non lus / Lus / Envoyés / Nouveau / Conversations + recherche.
- **Fil de conversation (Lot 6)** : `Comp_Conversation` (bulles chat), réutilise les tickets, Dirigeant ↔ Fournisseur. On peut répondre à un fil existant même si la relation a été retirée (`ConversationExiste`). Le fil ouvert se rafraîchit silencieusement (param `RefreshTick`).
- **Fournisseur voit le destinataire** : `DTO_CommandeAVenir.EstTiers`/`Destinataire` → badge « client externe (personne tierce) » si tiers, sinon nom du collaborateur.
- **Sécurité comptes** : reset mot de passe (`E_ResetMotDePasse`) + vérification email à l'inscription (`EmailVerifie` / `TokenVerification`) + changement de mot de passe dans « Mon profil ».
- **Optimisation Railway** : polling centralisé 60 s (`Comp_AutoRefresh`, pause si onglet en arrière-plan + bouton manuel) ; requêtes messagerie **projetées sans `bytea`** (`TicketApercu`) ; marquage lu / purge TTL via `ExecuteUpdate` / `ExecuteDelete`.
- ✅ **ROADMAP_V2 complète** : lots 0A → 6 tous livrés et déployés.
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
