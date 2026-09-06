# BTPSecure — Brief projet (lis-moi en premier)

## Identité
- **Marque publique** : **KEYDO** (nom affiché partout côté utilisateur : loader, header, emails, PDF, `<title>`). ⚠️ « BTPSecure » reste le **nom technique interne** — dossiers, namespaces, `.csproj`, repo GitHub, `JWT_EMETTEUR`/`JWT_AUDIENCE` — **NE PAS renommer** (renommer casserait le build et invaliderait les tokens JWT existants).
- **Type** : Blazor WebAssembly Hosted (.NET 10) + PostgreSQL + Railway
- **Path local** : `C:\Users\y1903\Desktop\BTPSecure`
- **Repo** : `github.com/YILDIZ-TOLGA/QR_CODE_BTP` (branche `main`)
- **Prod** : https://qrcodebtp-production.up.railway.app
- **Domaine perso** : https://www.keydopro.com (OVH → Railway, CNAME `www` + redirection apex ; ancien `codebtpsecure.cloud` abandonné)
- **Métier** : sécurisation d'achats BTP via QR codes (rôles : Admin / Dirigeant / Collaborateur / Fournisseur)

## Charte graphique (KEYDO)
- **Couleur primaire** : turquoise **#00C9B7** (remplace l'ancien bleu `#1565C0`). Déclinaisons dans `index.html :root` : `--rz-primary-light #33D6C7`, `--rz-primary-lighter #66E2D6`, `--rz-primary-dark #00A99A`, `--rz-primary-darker #008577`.
- **Fond des pages** : gris souris **#AEB4B9** (`--rz-layout-background-color` + `--rz-body-background-color`).
- **Cartes / panneaux** : **blancs** (`--rz-card-background-color` + `--rz-panel-background-color` = `#ffffff`). ⚠️ Les `RadzenCard Variant="Outlined"` sont **transparentes** par défaut → override obligatoire `.rz-card.rz-variant-outlined { background-color: var(--rz-card-background-color); }`, sinon le gris souris traverse l'intérieur des cartes.
- **Emails** (`S_Email.cs`) et **PDF** (`S_Pdf.cs`) : accent `#00C9B7`, marque « KEYDO » (fini le bleu / « BTPSecure »).
- **Logo officiel** : `Comp_Logo.razor` — tracé vectoriel fourni par la charte, deux formes : monogramme **« K »** (défaut) et **mot-symbole « KEYDO »** (`Complet="true"`). Il remplace partout l'ancienne icône `shield` et le texte « KEYDO ». Couleur = `currentColor` (donner la couleur au parent). ⚠️ Le **« O » est fait de deux contours** → `fill-rule="evenodd"` obligatoire, sinon le centre se remplit. Dupliqué à trois endroits par nécessité (médias différents) : `Comp_Logo.razor` (app), `index.html` (loader, avant le démarrage de Blazor), `S_Pdf.cs` (PDF, via `.Svg()` avec `fill` explicite car `currentColor` n'existe pas en PDF). **Emails laissés en texte** : Gmail et Outlook suppriment les SVG inline.
- ⚠️ **Dérogation assumée à la règle full Radzen** (comme les pièces jointes messagerie) : aucun composant Radzen ne rend un logo vectoriel → SVG inline dans `Comp_Logo.razor`. **Ne pas « corriger ».**

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
│   ├── Enums/         Enum_*.cs
│   └── Helpers/       H_*.cs        (logique partagée client + serveur)
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
- **Admin seed** : créé au démarrage si aucun admin, à partir des variables d'env `ADMIN_EMAIL` (défaut `admin_acc@keydopro.com`) + `ADMIN_PASSWORD` (**obligatoire, jamais en dur**). Sans `ADMIN_PASSWORD`, l'admin n'est pas créé.
- **Flow** : login → `S_Auth.Connecter` → JWT en localStorage (clé `"token"`) → `S_AuthStateProvider` lit + set `HttpClient.Authorization`
- **Claims JWT** : `NameIdentifier` (Id), `Email`, `Role`
- **Redirections post-login par rôle** : `/admin`, `/dirigeant`, `/collaborateur`, `/fournisseur`
- `MainLayout` s'abonne à `AuthenticationStateChanged` pour MAJ live de la sidebar
- 🔒 **`EstActif = false` coupe l'accès immédiatement** (voir piège 14), pas seulement la prochaine connexion. Seuls deux endroits bloquent un compte, et **les deux sont réversibles** : blocage fournisseur par l'admin (cascade sur les sous-comptes) et désactivation d'un sous-compte par son fournisseur principal. Les comptes Dirigeant / Collaborateur ne sont bloqués par aucune interface — « Bloquer » sur une entreprise agit sur `EstAutorisee` (droit de générer des codes), pas sur les comptes.

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
10. 🔒 **Le code est un PORTEUR** : quiconque connaît la valeur peut dépenser l'argent. Donc si on **change le destinataire** d'un code (`S_Code.Modifier`), il faut **RÉGÉNÉRER la valeur** — l'ancien destinataire l'a vue dans son espace (ou reçue par email pour un tiers) et pourrait encore l'utiliser. Changer **seulement le fournisseur** ne régénère pas (le fournisseur ne détient pas le code) mais remet `EstPrete`/`DatePrete` à zéro. Refusé sur code non actif ou permanent.
11. **Correspondance fournisseur : le SIRET décide, pas le SIREN.** Le SIREN est optionnel des deux côtés (carnet fournisseur ET inscription) ; exiger qu'il soit présent des deux côtés ou d'aucun **masquait silencieusement des commandes**. Le SIREN n'est comparé que **s'il est renseigné des deux côtés**. Un SIRET (14 chiffres) identique implique le même SIREN (ses 9 premiers chiffres) → aucune perte de sécurité. Règle appliquée dans `DAO_Code.ObtenirCommandesPourFournisseur` **et** `S_Code.MarquerPrete` (sinon la commande s'affiche mais « prête » est refusé).
12. **Textes saisis par l'utilisateur** : toujours `white-space: pre-wrap` **+ `overflow-wrap: anywhere`**. Sans le second, une longue suite de caractères **sans espace** n'a aucun point de coupure et **déborde** de la carte. Utiliser `H_TexteLibre` (troncature + « … » + style + curseur).
13. 🔒 **Un Responsable (Admin) ne voit JAMAIS la valeur d'un code destiné à un collègue.** Il le voit dans sa liste (suivi + révocation conservés) mais la valeur est remplacée par `••••-••••`, car le code est un **porteur** : la connaître = pouvoir la dépenser. Masquage **côté serveur** (la valeur n'atteint pas son navigateur, la cacher en CSS ne protégerait rien) en trois points : `S_Code.Creer` (valeur vidée dans le DTO retourné), `S_Code.Modifier` (`NouvelleValeur` vidée — sinon régénérer un code servirait de porte dérobée) et `ObtenirContexteDashboard` (vide `Valeur` pour tout code dont il n'est pas le destinataire). Il voit en clair les codes qui **lui** sont destinés, dont son code permanent. Le Dirigeant (`EstProprietaire`) garde la visibilité totale. `api/codes/dirigeant` est déjà `[Authorize(Roles="Dirigeant")]` et `notifications-dirigeant` ne renvoie pas la valeur.
14. 🔒 **Un compte bloqué perd l'accès IMMÉDIATEMENT**, sans attendre l'expiration de son jeton (24 h). Contrôle dans `OnTokenValidated` (Program.cs) : il couvre **toutes** les routes d'un coup, impossible d'en oublier une, et vaut aussi pour un appel API hors navigateur. Adossé à `S_CacheComptes` (singleton, TTL 5 min) pour ne pas lire la base à chaque requête — **toute modification de `EstActif` doit appeler `Invalider(id)`**, sinon le blocage attendrait l'expiration du cache. Côté client, `S_GestionnaireAuth` (DelegatingHandler) intercepte les 401, efface le jeton et renvoie vers `/connexion` : une connexion refusée renvoie **400**, donc un 401 signifie bien « jeton rejeté ».
15. **Un serveur de test local verrouille `publish/`** : si `dotnet publish` échoue sur `MSB3027 / fichier verrouillé`, c'est qu'une instance tourne encore (`Stop-Process` sur le PID qui écoute le port).

## Endpoints diagnostiques
- `GET /health` → `200 ok` (utilisé par Railway healthcheck)
- `GET /db-status` → état connexion BDD
- `GET /env-keys` → clés env présentes

## Variables d'env (Railway)
- `DATABASE_URL` (Postgres connection string)
- `JWT_CLE`, `JWT_EMETTEUR`, `JWT_AUDIENCE`, `JWT_DUREE_HEURES`
- **Emails Brevo (API HTTP, pas SMTP — Railway bloque le SMTP)** : `BREVO_API_KEY`, `SMTP_FROM` (`contact@keydopro.com`), `SMTP_FROM_NAME` (`KEYDO`), `SITE_URL` (`https://www.keydopro.com`). ⚠️ Le domaine `keydopro.com` doit être **authentifié (DKIM/DMARC)** dans le **même compte Brevo** que celui dont la `BREVO_API_KEY` est sur Railway.
- **Admin** : `ADMIN_EMAIL`, `ADMIN_PASSWORD` (seed du compte admin ; le mot de passe n'est plus dans le code)
- **Phase de test** : `CODE_ACCES` — **présente = portail beta actif** (splash « Code d'accès » avant le login) ; **la retirer désactive le portail** sans aucune modification de code. C'est le levier de sortie de beta.
- `ASPNETCORE_ENVIRONMENT=Production`, `PORT` (auto-fourni)
- `Program.cs` réinjecte les env vars dans `IConfiguration` au boot
- ⚠️ Brevo exige que l'IP de sortie Railway soit whitelistée (ou désactiver la restriction IP côté Brevo)

## Features récentes implémentées
- **Admin** : `EstAutorisee` sur `E_Entreprise` ; un Dirigeant ne peut générer de QR que si Admin a activé son entreprise
- **Multi-entreprises** : `E_CollaborateurEntreprise` (N-N, table physique `salaries_entreprises`) avec `Enum_StatutInvitation` (EnAttente / Acceptee / Refusee)
- **Invitations** : Dirigeant invite → Collaborateur accepte/refuse ; Collaborateur peut quitter (révoque ses codes)
- ⚠️ **Renommage Lot 0B (2026-07)** : `Patron`→`Dirigeant`, `Salarie`→`Collaborateur`, `Confiance`→`LibreService`, `E_SalarieEntreprise`→`E_CollaborateurEntreprise`. **Colonnes/tables physiques PostgreSQL inchangées** (`PatronId`, `SalarieId`, `salaries_entreprises`) via `HasColumnName`/`ToTable` dans `AppDbContext` — ne jamais toucher ces strings de mapping. Rôles JWT écrits par `.ToString()` → tokens émis avant le renommage exigent un re-login.
- **Rôles internes entreprise (Lot 1)** : `Enum_RoleEntreprise` {Collaborateur=1, Responsable=2, ResponsableAdmin=3} sur `E_CollaborateurEntreprise`. Responsable/RA = code permanent libre-service (`E_Code.EstPermanent`, régénéré à chaque validation, sans historique). Le RA peut créer des codes + a un tableau de bord (`Page_DashboardDirigeant`) mais **ne peut pas** révoquer son propre code ni celui d'un autre RA, ni changer les rôles.
- **Pré-remplissage du nom d'entreprise depuis le SIRET/SIREN** : `Comp_RechercheSiret.razor` (bouton « Retrouver l'entreprise depuis le SIRET »), branché sur les **4** formulaires qui saisissent un SIRET — inscription fournisseur, `Comp_DialogFournisseur`, `Page_CreerCode` (fournisseur à la volée) et `Page_DashboardDirigeant` (création d'entreprise, où l'**adresse** est aussi pré-remplie). Source : **`recherche-entreprises.api.gouv.fr`**, annuaire public **gratuit et sans clé d'API**. ⚠️ L'appel passe par **notre serveur** (`S_RechercheEntreprise`, endpoint anonyme car l'inscription n'est pas authentifiée) : pas de CORS, et un **cache mémoire 24 h** qui évite de faire limiter l'IP unique de Railway. ⚠️ **Le SIRET saisi prime sur le siège** : on lit `matching_etablissements[0]` et non `siege`, sinon une agence de Bordeaux se voit attribuer l'adresse du siège de Lezennes. Clé de Luhn vérifiée **avant** l'appel réseau (`H_Siret` dans `BTPSecure.Shared/Helpers`, partagé client + serveur). Le champ rempli **reste toujours modifiable** : entreprise individuelle non diffusible, société trop récente ou annuaire en panne ne doivent jamais bloquer une inscription.
- **Plafond de responsables par entreprise** : `E_Entreprise.LimiteResponsables` (**défaut 2**), réglable par l'admin (carte entreprise → bouton « Limite responsables : X / Y »). Plafond **commun** Responsable + Responsable Admin : avec 2, on peut avoir 2 RA, ou 1 RA + 1 R, ou 2 R. Comptés : liens **actifs ET invitation acceptée**. Contrôlé aux **deux** points d'attribution — `ChangerRole` et `CreerCollaborateur` (dans ce dernier **avant** toute création de compte, sinon on créerait un utilisateur pour ensuite le refuser). ⚠️ Passer de Responsable à Responsable Admin (ou l'inverse) **n'occupe pas de place supplémentaire** → on ne contrôle que si l'intéressé n'en occupait pas déjà une. Baisser la limite sous l'effectif actuel ne rétrograde personne, ça bloque juste les nouvelles nominations.
- **Se créer un code à soi-même** : la liste des destinataires de `Page_CreerCode` **excluait l'auteur** (`CollaborateurId != p_userId`) — ni le dirigeant ni un Responsable Admin ne pouvaient donc se générer un code ponctuel. L'exclusion est levée, et le **dirigeant est inséré en tête de liste à la main** (il n'existe pas dans `salaries_entreprises`). Côté validation, `S_Code.Creer` accepte explicitement `CollaborateurId == _entreprise.DirigeantId`, sinon `CollaborateurEstDansEntreprise` le rejetterait. L'entrée « moi-même » est signalée dans le libellé. La règle de masquage s'applique telle quelle : un code créé **pour soi** affiche sa valeur, un code créé pour un collègue reste `••••-••••` (sauf pour le dirigeant, qui voit tout).
- **Code permanent du dirigeant** : le Dirigeant a lui aussi un code permanent libre-service, comme un Responsable Admin. Créé **à la volée** dans `S_Code.ObtenirContexteDashboard` quand `EstProprietaire` (idempotent → couvre les entreprises déjà existantes, pas besoin de migration de données), et **uniquement si `EstAutorisee`** — sinon une entreprise non autorisée aurait un code utilisable alors que `Creer()` le lui interdit. Techniquement `CollaborateurId = DirigeantId` : toute la mécanique existante (régénération à la validation, non réattribuable) s'applique sans modification. Mis en avant dans une carte turquoise en haut du tableau de bord (sinon il se perdrait dans la liste). **Non révocable par son porteur** (garde serveur + boutons masqués) : il serait recréé au chargement suivant.
- **Inscription 2 étapes (Lot 2)** : cartes Dirigeant / Collaborateur / Fournisseur. Création de collaborateur par le Dirigeant (email obligatoire, mot de passe temporaire envoyé par mail).
- **Logique codes (Lot 3)** : libre-service = **usage unique** ; type Liste avec achats supplémentaires **0/50/100/200 € HT** ; code pour un **tiers externe** (`E_Code.EmailTiers`, envoyé par mail). Type par défaut = Liste, case à cocher pour passer en Libre-service.
- ⚠️ **Validité : plus fixe depuis 2026-07** (la roadmap disait « 24 h uniquement, pas d'option » — décision changée par l'utilisateur). Champ `RadzenNumeric` **24 h par défaut**, borné **1 h → 168 h (7 j)** côté client **et** serveur. `DTO_CreerCode.DureeValiditeHeures` existait déjà mais était ignoré.
- **Espace fournisseur (Lot 4)** : validation admin des fournisseurs (`E_Utilisateur.EstValide`) ; **sous-comptes** (`ParentFournisseurId`, SIRET partagé) ; **blacklist** par email (`E_Blacklist`) ; navigation Accueil / À préparer / Prêtes ; notification « commande prête ».
- **Messagerie / tickets (Lot 5)** : `E_Ticket` (pièce jointe en `bytea`, **TTL 24 h** via `S_NettoyageTickets` BackgroundService) ; **annuaire** selon l'écosystème ; destinataire interne OU email externe (Brevo) ; **badge non-lus** sidebar. `Page_Messagerie` : vues Non lus / Lus / Envoyés / Nouveau / Conversations + recherche.
- **Fil de conversation (Lot 6)** : `Comp_Conversation` (bulles chat), réutilise les tickets, Dirigeant ↔ Fournisseur. On peut répondre à un fil existant même si la relation a été retirée (`ConversationExiste`). Le fil ouvert se rafraîchit silencieusement (param `RefreshTick`).
- **Fournisseur voit le destinataire** : `DTO_CommandeAVenir.EstTiers`/`Destinataire` → badge « client externe (personne tierce) » si tiers, sinon nom du collaborateur.
- **Sécurité comptes** : reset mot de passe (`E_ResetMotDePasse`) + vérification email à l'inscription (`EmailVerifie` / `TokenVerification`) + changement de mot de passe dans « Mon profil ».
- **Optimisation Railway** : polling centralisé 60 s (`Comp_AutoRefresh`, pause si onglet en arrière-plan + bouton manuel) ; requêtes messagerie **projetées sans `bytea`** (`TicketApercu`) ; marquage lu / purge TTL via `ExecuteUpdate` / `ExecuteDelete`.
- ✅ **ROADMAP_V2 complète** : lots 0A → 6 tous livrés et déployés (y compris le point différé du Lot 2, la pop-up de création de collaborateur).
- **Création de collaborateur mutualisée** : `Comp_FormCreerCollaborateur` (formulaire unique) utilisé par la page dédiée **et** par `Comp_DialogCreerCollaborateur` (pop-up). Points d'entrée : tableau de bord (bouton **Créer**, à côté de **Inviter** = rattacher un compte existant) et **page de création de code** (création à la volée + pré-sélection automatique du nouveau collaborateur).
- **Le Responsable Admin peut créer des collaborateurs** : `C_Entreprise` est passé en `[Authorize]` avec `[Authorize(Roles="Dirigeant")]` sur chaque action **sauf** `creer-collaborateur` (Dirigeant + Collaborateur). Le service résout l'entreprise via le Dirigeant **ou** `ObtenirPremierLienResponsableAdmin`. **Anti-escalade : un RA ne peut pas créer un autre RA** (bloqué serveur + rôle masqué via `PeutCreerResponsableAdmin`).
- **Séparation visuelle des rôles (dashboard)** : une **section par rôle** (en-tête + compteur + rappel des droits, section vide masquée), **bordure gauche colorée** sur les cartes, **compteurs par rôle** dans l'en-tête entreprise.
- **Réattribution d'un code généré** (`S_Code.Modifier`, `Comp_DialogModifierCode`) : change destinataire et/ou fournisseur. ⚠️ Voir la règle de sécurité « code = porteur » dans les pièges.
- **Message contextuel** : bouton « Envoyer un message » sur les fiches collaborateur (dashboard) et fournisseur (Mes fournisseurs) → `Comp_DialogEnvoyerMessage`, qui **résout seul** le destinataire (compte interne trouvé dans l'annuaire, sinon envoi par email) et gère les pièces jointes.
- **Emails fiabilisés** : `AjouterCollaborateur` (« Inviter ») n'envoyait **aucun** email malgré son message de succès → `EnvoyerInvitationCollaborateur` ajouté. Les envois liés à la création ne sont **plus en fire-and-forget** : en cas d'échec Brevo, le **mot de passe temporaire est rendu au créateur** (il n'existe que dans cet email, sinon le compte est inutilisable).
- **Notifications de changement de statut** : `E_Notification` (table `notifications`) + `S_Notification`. Le dirigeant change le rôle d'un collaborateur (ou le retire de l'entreprise) pendant que celui-ci est déconnecté → la notification est **stockée**, puis affichée en toast Radzen à sa **prochaine connexion** par `MainLayout.OnAfterRenderAsync` (pas `OnInitializedAsync` : `<RadzenComponents />` doit déjà être rendu, sinon le toast est perdu), et marquée lue dans la foulée pour ne pas réapparaître.
- **Historique des validations + onglet « Codes permanents »** (Dirigeant seul) : `E_ValidationCode` (table `validations_codes`) enregistre **chaque** utilisation d'un code. ⚠️ Indispensable car un code permanent **réutilise la même ligne** et écrasait sa validation précédente : avant cette table, seule la **dernière** utilisation était connue. Les champs sont des **instantanés** pris **avant** la régénération de la valeur (valeur utilisée, n° de commande, achats suppl., porteur, validateur) — le code change après, l'historique doit refléter l'instant du passage. Écrit dans `S_Code.Valider`, **point de passage unique** (`ValiderPourCommande` y délègue), **après** la sauvegarde du code et dans un `try/catch` : une trace en échec ne doit jamais annuler une validation déjà acquise au comptoir. Clés étrangères en `Restrict` : l'historique ne disparaît pas avec un code ou un compte. Page `Page_GestionCodesPermanents` → porteurs (dirigeant en tête) puis détail cliquable ; endpoint `[Authorize(Roles="Dirigeant")]` **et** bornage serveur à l'entreprise possédée — un Responsable Admin ne surveille pas ses collègues. ⚠️ **L'historique démarre à la mise en service** : les passages antérieurs sont perdus, la page le dit à l'utilisateur.
- **Déconnexion automatique après 10 min d'inactivité** (`Comp_Inactivite`), pour les seuls comptes pouvant engager de l'argent : **Dirigeant, Responsable et Responsable Admin**. Avertissement Radzen à 1 min du terme, puis fermeture de session et retour sur `/connexion?inactif=1` qui **explique** ce qui s'est passé. ⚠️ « Responsable » n'est **pas** un rôle JWT (le jeton dit « Collaborateur ») → `DTO_Profil.EstResponsable` est calculé par `C_Profil` et lu par `MainLayout`. Le suivi d'activité vit dans `index.html` (`window.keydoInactivite`, dérogation d'infra comme le helper de téléchargement) : aucun composant Radzen ne capte l'activité globale de la page. Blazor **interroge** la durée écoulée toutes les 15 s — pas de rappel JS → .NET, donc pas de `DotNetObjectReference` à gérer. Si le suivi est indisponible, on ne déconnecte **jamais** sur une incertitude. Onglet en arrière-plan : les minuteurs sont bridés par le navigateur, la déconnexion peut donc arriver avec jusqu'à une minute de retard.
- **Portail d'accès beta** : `Comp_AccesBeta` + `S_Acces` + `C_Acces`. Splash « Code d'accès » **avant le login**, monté dans `MainLayout` — chokepoint unique qui intercepte **toute** URL, y compris une adresse tapée à la main. Le code réel n'est **jamais dans le bundle WASM** : le client demande seulement « le portail est-il actif ? » puis « ce code est-il bon ? ». Réponse mémorisée dans `localStorage` (`acces_beta`) → saisie une seule fois. Piloté par la variable `CODE_ACCES` (voir Variables d'env).
- **Blocage / déblocage fournisseur (admin)** : vrai toggle — l'ancien bouton « Désactiver » était **à sens unique**, aucun retour possible. Bloquer un **compte principal bloque tous ses sous-comptes** ; le débloquer les débloque. Un sous-compte ne peut pas être débloqué seul tant que son principal l'est (garde serveur **et** bouton désactivé).
- **Limite de sous-comptes fournisseur** : `E_Utilisateur.LimiteSousComptes` (**défaut 3**), réglable par l'admin. Seuls les sous-comptes **actifs** comptent → désactiver libère une place. Vérifiée à la création **et à la réactivation** (sinon on contournerait en désactivant/réactivant). Badge « X / Y » + alerte « Nombre de sous-comptes atteint » sur la page fournisseur.
- **Inscription fournisseur** : **nom de l'entreprise obligatoire**, nom/prénom **optionnels** (à défaut, le compte porte le nom de la société ; les emails saluent avec ce nom pour éviter « Bonjour , »). Les sous-comptes **héritent** de la société du principal.
- **Entreprise du dirigeant créée dès l'inscription** : le champ « Nom de l'entreprise » est demandé au formulaire, l'`E_Entreprise` est créée dans la foulée. L'écran « Créer votre entreprise » du tableau de bord ne subsiste qu'en **filet de sécurité** pour d'anciens comptes. La société d'un dirigeant vit **uniquement** dans `E_Entreprise` (`E_Utilisateur.NomSociete` reste réservé aux fournisseurs — pas de duplication).
- **Sidebar conditionnelle** : cachée si non connecté ; menu burger caché aussi
- **Highlight exact** des items menu : `Match="NavLinkMatch.All"`
- **Loader index.html** stylisé : monogramme « K » dans un carré glassmorphism + mot-symbole KEYDO, dégradé turquoise. ⚠️ Écrit en dur dans `index.html` car il s'affiche **avant** le démarrage de Blazor : `Comp_Logo` n'y est pas utilisable.
- **Persistance session** : `Page_Connexion.OnInitializedAsync` redirige si déjà authentifié

## Sources UNIQUES à réutiliser (ne pas re-dupliquer)

**Helpers** — côté client dans `BTPSecure.Client/Services/`, **sauf `H_Siret`** qui vit dans `BTPSecure.Shared/Helpers/` car serveur et client l'utilisent tous les deux :
| Helper | Rôle |
|---|---|
| `H_Siret` *(Shared)* | Nettoyage + validation de la clé de Luhn SIREN/SIRET, hors ligne. Exception La Poste (`356000000`) gérée. |
| `H_RoleEntreprise` | Libellé / pluriel / couleur / icône / badge / description des droits d'un rôle |
| `H_TexteLibre` | Seuil de troncature (140), « … », style `pre-wrap + overflow-wrap`, curseur si cliquable |
| `H_Code` | Formatage de la saisie d'un code : majuscules, `-` auto après 4 caractères, 8 max |
| `H_TypeCode` | Libellé du type de code (LibreService → « Libre-service ») |

**Composants réutilisables** (`BTPSecure.Client/Components/`) :
| Composant | Rôle |
|---|---|
| `Comp_Logo` | Logo KEYDO : monogramme « K » ou mot-symbole complet (`Complet="true"`) |
| `Comp_AutoRefresh` | Polling 60 s + pause si onglet caché + bouton manuel |
| `Comp_SelecteurPieceJointe` | Choix + validation d'une PJ (JPG/PNG/PDF, 5 Mo), `@bind-Fichier` |
| `Comp_FormCreerCollaborateur` | Formulaire de création (page + pop-up), `OnCree` |
| `Comp_DialogEnvoyerMessage` | Envoi rapide d'un message depuis n'importe quelle fiche |
| `Comp_DialogTexte` | Affichage d'un texte long en dialogue |
| `Comp_ListeCommandes` | Liste des commandes fournisseur (à préparer / prêtes) |
| `Comp_Conversation` | Fil de discussion en bulles + réponse |
| `Comp_RechercheSiret` | Bouton « Retrouver l'entreprise depuis le SIRET », `OnTrouve` laisse le parent choisir les champs à remplir |
| `Comp_AccesBeta` | Splash du portail de phase de test (piloté par `CODE_ACCES`) |
| `Comp_Logo` *(cf. charte)* | Logo KEYDO, deux formes |
| `Comp_ResultatValidation` | Détail d'une validation (collaborateur, matériaux, PDF), partagé accueil fournisseur + pop-up |
| `Comp_DialogLimiteResponsables` | Réglage admin du plafond Responsable + Responsable Admin |
| `Comp_DialogLimiteSousComptes` | Réglage admin de la limite de sous-comptes d'un fournisseur |

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
curl -s https://www.keydopro.com/health
curl -s https://www.keydopro.com/api/acces/statut          # {"actif":true} = portail beta actif
git commit --allow-empty -m "trigger redeploy" && git push
```

**Tester le build publié en local avant de pousser** (recommandé : c'est l'artefact réellement déployé, trimmé) :
```bash
cd /c/Users/y1903/Desktop/BTPSecure/publish
ASPNETCORE_ENVIRONMENT=Production dotnet BTPSecure.Server.dll --urls http://localhost:5199
# PostgreSQL local absent : les erreurs Npgsql au démarrage sont NORMALES,
# l'app sert quand même le WASM et les endpoints sans base.
```
Puis arrêter avant tout nouveau `publish`, sinon la DLL reste verrouillée (piège 15) :
```powershell
Get-NetTCPConnection -LocalPort 5199 -State Listen | Select-Object -First 1 -ExpandProperty OwningProcess | Stop-Process -Force
```
