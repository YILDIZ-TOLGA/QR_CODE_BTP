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
- **Couleur primaire** : cyan **#00D6FF** (historique : bleu `#1565C0` → turquoise `#00C9B7` → cyan `#00D6FF`). Déclinaisons dans `index.html :root` : `--rz-primary-light #33DEFF`, `--rz-primary-lighter #66E6FF`, `--rz-primary-dark #00B4D6`, `--rz-primary-darker #008CA7`. ⚠️ Les déclinaisons ne sont **pas arbitraires** : `light` = 20 % de blanc, `lighter` = 40 % de blanc, `dark` = × 0,8412, `darker` = × 0,6560 — ratios repris de l'ancienne charte. Recalculer avec **ces mêmes ratios** à tout changement de teinte, sinon la gamme se déséquilibre.
- **Bleu « Info » du thème Radzen aligné sur la charte** : `material-base.css` définit `--rz-info: #2196F3`, un vrai bleu qui jurait avec la marque et qui sortait partout (`BadgeStyle.Info`, `AlertStyle.Info` dans `Comp_ListeCommandes`, `Comp_ResultatValidation`, `MainLayout`…). Il est surchargé dans `index.html :root` aux mêmes valeurs que la primaire. ⚠️ **Conséquence assumée** : un badge Info n'est plus visuellement distinct d'un élément primaire — ne pas « corriger », c'est un choix.
- ⚠️ **Contraste** : le cyan #00D6FF est très lumineux → le **texte blanc dessus tombe à ~1,7:1** (l'ancien turquoise était déjà faible, ~2,1:1 ; WCAG AA demande 4,5:1). Cela concerne les boutons primaires, les entêtes d'emails et les boutons d'action des emails, tous en `color: white`. Sur fond blanc à l'inverse, le cyan en **texte** est illisible. Pour un vrai contraste, utiliser `--rz-primary-darker` (#008CA7) ou du **texte foncé sur fond cyan**.
- **Fond des pages** : gris souris **#AEB4B9** (`--rz-layout-background-color` + `--rz-body-background-color`).
- **Cartes / panneaux** : **blancs** (`--rz-card-background-color` + `--rz-panel-background-color` = `#ffffff`). ⚠️ Les `RadzenCard Variant="Outlined"` sont **transparentes** par défaut → override obligatoire `.rz-card.rz-variant-outlined { background-color: var(--rz-card-background-color); }`, sinon le gris souris traverse l'intérieur des cartes.
- **Emails** (`S_Email.cs`, 23 occurrences) et **PDF** (`S_Pdf.cs` : logo SVG, filet, valeur du code) : accent `#00D6FF`, marque « KEYDO ». ⚠️ Ces deux fichiers portent la couleur **en dur** — ils ne voient pas les variables CSS → tout changement de charte doit les inclure. Les `.razor`, eux, utilisent `var(--rz-primary)` et suivent automatiquement.
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
- `Enum_Role` : `Admin=0`, `Dirigeant=1`, `Collaborateur=2`, `Fournisseur=3`, `ApporteurAffaire=4`
- 🔒 **Liste blanche des rôles à l'inscription** (`S_Auth.Inscrire`) : le rôle arrive **du navigateur**. Sans garde, un `POST api/auth/inscription` avec `"Role": 0` créait un **administrateur** — élévation de privilège complète. Seuls **Dirigeant, Collaborateur, Fournisseur et ApporteurAffaire** s'auto-inscrivent ; `Admin` ne naît que du seed de démarrage. **Toute nouvelle valeur de `Enum_Role` doit être ajoutée explicitement à cette liste** pour être inscriptible — le refus est le défaut, c'est voulu.
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
16. **Vérifier une chaîne dans un assembly publié : DEUX encodages.** C'est le principal contrôle local disponible (pas de PostgreSQL ici), autant ne pas conclure à tort. Les **littéraux de code** vivent dans le heap `#US` en **UTF-16LE** ; les **arguments d'attributs** (`[Route("api/x")]`, `[Authorize(Roles = "Y")]`) vivent dans le heap `#Blob` en **UTF-8**. Chercher une route en UTF-16 renvoie donc **0 alors que l'endpoint existe** — piège vécu. Tester les deux :
    ```python
    d = open('publish/BTPSecure.Server.dll','rb').read()
    d.count('api/apporteur'.encode('utf-8'))      # routes, rôles  -> UTF-8
    d.count('Message d'erreur'.encode('utf-16-le'))  # littéraux  -> UTF-16LE
    ```
    Le plus fiable reste de **lancer l'artefact publié** et d'interroger l'endpoint (401 = présent et protégé).
17. **`RadzenFormField` + `RadzenDropDown` : une valeur « vide » superpose le label à la valeur.** Le label ne remonte que grâce à la règle CSS `.rz-form-field-content > :not(.rz-state-empty) ~ .rz-form-field-label`. Or Radzen pose `rz-state-empty` sur une liste dont la valeur liée est une **chaîne vide** — le label reste alors en place et **se superpose au texte sélectionné**, bien que la liste affiche correctement son libellé. → Pour une option « tout / aucun filtre », utiliser une **sentinelle non vide** (`"TOUS"`), jamais `string.Empty`. L'icône `<Start>` n'y est pour rien (vérifié). **Méthode de diagnostic** : une page Razor jetable en `@page` anonyme, publiée et ouverte dans le navigateur, suffit à trancher ce genre de question **sans base de données** — comparer les variantes côte à côte puis lire `el.classList` plutôt que de corriger au jugé.

## Endpoints diagnostiques
- `GET /health` → `200 ok` (utilisé par Railway healthcheck)
- `GET /db-status` → état connexion BDD
- `GET /env-keys` → clés env présentes

## Variables d'env (Railway)
- `DATABASE_URL` (Postgres connection string)
- `JWT_CLE`, `JWT_EMETTEUR`, `JWT_AUDIENCE`, `JWT_DUREE_HEURES`
- **Emails Brevo (API HTTP, pas SMTP — Railway bloque le SMTP)** : `BREVO_API_KEY`, `SMTP_FROM` (`contact@keydopro.com`), `SMTP_FROM_NAME` (`KEYDO`), `SITE_URL` (`https://www.keydopro.com`). ⚠️ Le domaine `keydopro.com` doit être **authentifié (DKIM/DMARC)** dans le **même compte Brevo** que celui dont la `BREVO_API_KEY` est sur Railway.
- **Admin** : `ADMIN_EMAIL`, `ADMIN_PASSWORD` (seed du compte admin ; le mot de passe n'est plus dans le code)
- **Session unique** : `SESSION_UNIQUE_EXCLUSIONS` — rôles exemptés, séparés par des virgules (ex. `Fournisseur`). **Absente = tous les rôles concernés** (comportement actuel voulu). Lue **au démarrage** → un changement exige un redémarrage du service.
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
- **Messagerie : annuaire élargi + RGPD**. ⚠️ **L'annuaire EST le contrôle d'accès** : `S_Ticket.Envoyer` refuse tout destinataire absent de l'annuaire de l'expéditeur — élargir l'annuaire élargit donc les permissions, il n'y a rien d'autre à modifier (et rien à oublier). Règles : **tous les membres d'une même entreprise** se parlent entre eux (dirigeant ↔ collaborateurs ↔ collaborateurs) ; **Dirigeant / Responsable / Responsable Admin** dialoguent avec les **fournisseurs** de leur entreprise, **dans les deux sens** (le fournisseur voit le dirigeant ET les responsables). Un collaborateur simple n'apparaît pas dans l'annuaire d'un fournisseur : il n'a pas de relation commerciale avec lui.
- **RGPD (messagerie)** — quatre mesures concrètes : **(1) minimisation** — `DTO_ContactAnnuaire` **ne contient plus d'email** ; il n'était pas affiché mais servait à `Comp_DialogEnvoyerMessage` pour retrouver un compte, ce que fait désormais `POST api/tickets/resoudre-destinataire` (le navigateur interroge **une** adresse qu'il connaît déjà au lieu de télécharger celle de tout le monde ; **POST et non GET**, pour qu'aucun email ne finisse dans les journaux). **(2) Exactitude** — un compte **désactivé** est exclu de l'annuaire. **(3) Conservation limitée** — tickets et pièces jointes supprimés à **24 h** (`S_NettoyageTickets`), déjà en place. **(4) Transparence** — bandeau sur `Page_Messagerie` annonçant la durée de conservation et qui peut lire. Confidentialité déjà correcte : `ObtenirPieceJointe` n'autorise que l'expéditeur ou le destinataire.
- **Notification « commande prête » au destinataire** : elle ne remonte plus au seul dirigeant. `S_Code.ObtenirNotificationsDirigeant` **dispatche selon le profil** — dirigeant d'une entreprise (`ObtenirParDirigeantId` non nul) → toutes les commandes prêtes de son entreprise ; sinon → **uniquement celles qui lui sont destinées** (`CollaborateurId`), cohérent avec la règle « un Responsable ne suit pas les retraits de ses collègues ». Endpoint et page ouverts à `Dirigeant,Collaborateur`, le **bornage reste serveur**. Badge de compteur dans la sidebar pour les deux rôles. ⚠️ Le compteur passe par un **COUNT dédié** (`CompterNotificationsPour…`), sans `Include` ni `SaveChanges` : recharger la liste complète pour un badge interrogé toutes les 60 s par utilisateur coûterait cher sur Railway.
- **Session unique (un seul appareil connecté par compte)** : `E_Utilisateur.SessionId`, régénéré à **chaque connexion**, embarqué dans le jeton et comparé à chaque requête dans `OnTokenValidated`. Une nouvelle connexion évince donc la précédente : l'ancien appareil est refusé à sa requête suivante (au pire ~60 s grâce au polling déjà en place), son jeton est effacé et il revient sur `/connexion?remplace=1` qui **explique** l'éviction. ⚠️ **La connexion DOIT invalider le cache** (`S_CacheComptes`) : sans ça l'ancien appareil resterait accepté jusqu'à 5 min. ⚠️ **Claim nommé `keydo_sid`, pas `sid`** : `sid` est un nom JWT réservé, susceptible d'être renommé par le framework à la lecture — la comparaison échouerait alors **toujours**, déconnectant tout le monde en boucle. ⚠️ `SessionId` vide = aucune connexion depuis la mise en service → contrôle **non appliqué**, sinon le déploiement déconnecterait tous les utilisateurs d'un coup. Le motif est transmis au navigateur par l'en-tête `X-Session-Remplacee` posé dans `OnChallenge` (uniquement dans ce cas), lu par `S_GestionnaireAuth`. Exemptions par rôle via `SESSION_UNIQUE_EXCLUSIONS`. Plusieurs onglets d'un **même** navigateur partagent le jeton : ils ne se gênent pas.
- **Historique des validations + onglet « Codes permanents »** (Dirigeant seul) : `E_ValidationCode` (table `validations_codes`) enregistre **chaque** utilisation d'un code. ⚠️ Indispensable car un code permanent **réutilise la même ligne** et écrasait sa validation précédente : avant cette table, seule la **dernière** utilisation était connue. Les champs sont des **instantanés** pris **avant** la régénération de la valeur (valeur utilisée, n° de commande, achats suppl., porteur, validateur) — le code change après, l'historique doit refléter l'instant du passage. Écrit dans `S_Code.Valider`, **point de passage unique** (`ValiderPourCommande` y délègue), **après** la sauvegarde du code et dans un `try/catch` : une trace en échec ne doit jamais annuler une validation déjà acquise au comptoir. Clés étrangères en `Restrict` : l'historique ne disparaît pas avec un code ou un compte. Page `Page_GestionCodesPermanents` → porteurs (dirigeant en tête) puis détail cliquable ; endpoint `[Authorize(Roles="Dirigeant")]` **et** bornage serveur à l'entreprise possédée — un Responsable Admin ne surveille pas ses collègues. ⚠️ **L'historique démarre à la mise en service** : les passages antérieurs sont perdus, la page le dit à l'utilisateur.
- **Déconnexion automatique après 10 min d'inactivité** (`Comp_Inactivite`), pour les seuls comptes pouvant engager de l'argent : **Dirigeant, Responsable et Responsable Admin**. Avertissement Radzen à 1 min du terme, puis fermeture de session et retour sur `/connexion?inactif=1` qui **explique** ce qui s'est passé. ⚠️ « Responsable » n'est **pas** un rôle JWT (le jeton dit « Collaborateur ») → `DTO_Profil.EstResponsable` est calculé par `C_Profil` et lu par `MainLayout`. Le suivi d'activité vit dans `index.html` (`window.keydoInactivite`, dérogation d'infra comme le helper de téléchargement) : aucun composant Radzen ne capte l'activité globale de la page. Blazor **interroge** la durée écoulée toutes les 15 s — pas de rappel JS → .NET, donc pas de `DotNetObjectReference` à gérer. Si le suivi est indisponible, on ne déconnecte **jamais** sur une incertitude. Onglet en arrière-plan : les minuteurs sont bridés par le navigateur, la déconnexion peut donc arriver avec jusqu'à une minute de retard.
- **Blocage / déblocage fournisseur (admin)** : vrai toggle — l'ancien bouton « Désactiver » était **à sens unique**, aucun retour possible. Bloquer un **compte principal bloque tous ses sous-comptes** ; le débloquer les débloque. Un sous-compte ne peut pas être débloqué seul tant que son principal l'est (garde serveur **et** bouton désactivé).
- **Limite de sous-comptes fournisseur** : `E_Utilisateur.LimiteSousComptes` (**défaut 3**), réglable par l'admin. Seuls les sous-comptes **actifs** comptent → désactiver libère une place. Vérifiée à la création **et à la réactivation** (sinon on contournerait en désactivant/réactivant). Badge « X / Y » + alerte « Nombre de sous-comptes atteint » sur la page fournisseur.
- **Inscription fournisseur** : **nom de l'entreprise obligatoire**, nom/prénom **optionnels** (à défaut, le compte porte le nom de la société ; les emails saluent avec ce nom pour éviter « Bonjour , »). Les sous-comptes **héritent** de la société du principal.
- **Entreprise du dirigeant créée dès l'inscription** : le champ « Nom de l'entreprise » est demandé au formulaire, l'`E_Entreprise` est créée dans la foulée. L'écran « Créer votre entreprise » du tableau de bord ne subsiste qu'en **filet de sécurité** pour d'anciens comptes. La société d'un dirigeant vit **uniquement** dans `E_Entreprise` (`E_Utilisateur.NomSociete` reste réservé aux fournisseurs — pas de duplication).
- **Mémos / « Prêt de matériel »** (`E_Memo`, `S_Memo`, `C_Memo`, `Page_Memo`, `Comp_DialogMemo`) : pense-bête **strictement personnel**, ouvert à **tout compte connecté** (`[Authorize]` sans rôle). Chaque lecture, enregistrement et suppression est borné à `UtilisateurId` **côté serveur** — personne ne voit les notes d'un autre, pas même le dirigeant ou l'admin. ⚠️ **Contrairement aux tickets, un mémo n'expire pas** : pas de TTL 24 h, `S_NettoyageTickets` ne le touche pas. Bornes de saisie : titre 200 caractères, contenu 10 000. Recherche client sur titre + contenu.
- **Historique des validations côté fournisseur** (`Page_HistoriqueValidations`, `/fournisseur/historique`, `[Authorize(Roles="Fournisseur")]`) : ce qu'a validé le compte principal **et ses sous-comptes**. ⚠️ À ne pas confondre avec `Page_GestionCodesPermanents` (côté **dirigeant**, via `C_HistoriqueCode`) : deux publics, deux endpoints, deux bornages distincts.
- **Apporteur d'affaires (rôle `ApporteurAffaire`)** : amène des dirigeants via un **code de parrainage** personnel et touche une commission par filleul. Inscription par une 4ᵉ carte sur `Page_Inscription` ; **nom et prénom obligatoires** (ils fabriquent le code). Compte actif immédiatement, **sans validation admin** (contrairement au fournisseur) : rien ne le justifiait, l'argent ne se décide qu'au parrainage. Pas d'entrée **Messagerie** dans sa sidebar : il n'appartient à aucune structure, donc à aucun annuaire — le menu serait vide. Tableau de bord `Page_DashboardApporteur` (`/apporteur`) : son code en grand, trois compteurs (amenés / en attente / cumul €) et la liste de ses filleuls.
- **Code de parrainage, et comment les doublons sont exclus** (`H_CodeParrainage`, dans `Shared/Helpers` car client + serveur) : **initiales prénom+nom puis numéro d'ordre** — Tolga YILDIZ → `TY-01`. Le numéro est un **compteur par préfixe** : un second porteur de deux initiales `TY` reçoit `TY-02`, quel que soit son nom. ⚠️ **L'unicité est garantie par l'INDEX UNIQUE en base, pas par le calcul** : deux inscriptions simultanées aux mêmes initiales liraient le même maximum — `DAO_Parrainage.CreerApporteur` rattrape la collision en relisant le maximum et en retentant (5 essais, l'entité n'est ajoutée au tracker qu'une fois). ⚠️ **Pas de `string.Normalize(FormD)`** pour retirer les accents : le WASM client est trimmé et sa globalisation peut être réduite — table d'équivalences explicite (français + turc, utile pour « YILDIZ »), et **garde-fou ASCII** qui remplace par `X` tout caractère n'aboutissant pas à une lettre A-Z (le « ı » turc, un prénom non latin…). La saisie est **tolérante** (`ty01`, `ty 01`, `TY-01` → `TY-01`) : le code circule à l'oral.
- **Parrainage — la commission se fixe À LA VALIDATION, une seule fois** (`E_Parrainage`, table `parrainages`) : à l'inscription d'un dirigeant avec un code, le lien naît **`EnAttente`** avec un montant à 0. Il passe **`Valide`** quand un admin **autorise l'entreprise du filleul** (`EstAutorisee`) — c'est là que l'admin saisit le montant, dans `Comp_DialogAutoriserParrainage` (pré-rempli à 20 €, librement modifiable ; **il n'y a pas de réglage global** du montant, chaque validation décide la sienne). ⚠️ **L'autorisation est une BASCULE** : bloquer puis réautoriser ne doit pas repayer l'apporteur → seul un parrainage encore `EnAttente` est touché, et le montant n'est **jamais** recalculé ensuite. ⚠️ Un **code inconnu REFUSE l'inscription** au lieu d'être ignoré : ignoré en silence, l'apporteur perdrait sa commission sans que personne ne s'en aperçoive. Un compte apporteur **désactivé** voit son code cesser de fonctionner. Index **unique sur `FilleulId`** : un filleul n'est parrainé qu'une fois. Clés étrangères en `Restrict` : la trace d'une commission ne disparaît pas avec un compte. Montant en `numeric(10,2)`, **jamais un `double`** (arrondis sur des euros). Côté admin, une section « Apporteurs d'affaires » (`Page_Admin`) dit qui payer et combien, et la carte entreprise affiche le parrainage **avant** le clic sur « Autoriser ». RGPD : `DTO_FilleulAffichage` **ne porte ni email ni téléphone** du filleul — l'apporteur a droit au suivi de sa commission, pas aux coordonnées des gens qu'il a amenés.
- **Suppression définitive d'un compte par l'admin** (`S_Suppression`, `DAO_Suppression`, `Comp_DialogSupprimerCompte`, section « Comptes utilisateurs » de `Page_Admin`). ⚠️ **Les 18 clés étrangères vers `utilisateurs` sont toutes en `Restrict`** — c'est voulu — donc un `DELETE` nu échoue dans la quasi-totalité des cas : il faut effacer les dépendances **dans l'ordre**. Principe directeur : **un admin peut effacer les données d'un compte, pas celles d'autrui.**
- **Suppression — ce qui la REFUSE** (recalculé côté serveur avant d'agir, l'aperçu affiché ayant pu vieillir) : compte **Admin** (sinon on se verrouille dehors), **son propre compte**, compte ayant **validé des commandes** (`ValidationsCodes.ValidateurId` — ce serait l'historique d'achat d'autres entreprises), compte ayant des **validations à son nom** (`PorteurId`), **dirigeant dont l'entreprise a un historique de validations**, **parrainage à commission déjà validée** (trace financière), **fournisseur avec des sous-comptes** (à traiter d'abord). Chaque refus est rendu **avec sa raison** et renvoie vers le **blocage**, qui coupe l'accès sans rien détruire.
- **Suppression — ce qu'elle fait** : efface notifications, mémos, jetons de reset, messages, blacklist, parrainages en attente, rattachements, codes qui lui étaient destinés, carnet fournisseur, et **son entreprise s'il est dirigeant** (les collaborateurs sont détachés, leurs comptes subsistent). ⚠️ **Les codes d'autres entreprises où il n'est qu'intervenant sont DÉTACHÉS, pas détruits** (`CreateurId`/`FournisseurId` mis à `null`) : un code vivant ne doit pas disparaître parce que son auteur est supprimé. ⚠️ **Tout est dans UNE transaction** : si l'ordre des dépendances comporte un oubli, l'opération est **annulée en entier** et la cause racine est remontée à l'admin — jamais un compte à moitié supprimé. ⚠️ **`S_CacheComptes.Invalider`** est obligatoire après coup, sinon le jeton du compte supprimé reste accepté jusqu'à 5 min (`OnTokenValidated` traite déjà le compte absent comme inactif).
- **Sur-confirmation en deux temps** (`Comp_DialogSupprimerCompte`) : étape 1, **l'étendue des dégâts** chiffrée (codes, messages, mémos… seuls les volumes non nuls sont listés) et l'alerte rouge si une entreprise disparaît ; étape 2, **l'email du compte à recopier** — bouton inactif jusqu'à correspondance exacte, et **vérification refaite côté serveur** (un contrôle seulement dans le navigateur ne protège rien). Ce n'est pas une formalité : c'est ce qui empêche de supprimer la mauvaise ligne d'une liste.
- **Diffusion de notifications par rôle** (onglet admin « Envoi de notification », `/admin/notifications`) : l'admin rédige un message, coche un ou plusieurs **rôles destinataires**, fixe une **fenêtre de dates**, et les comptes concernés le voient à leur connexion. ⚠️ **UNE ligne (`E_NotificationGlobale`), pas une copie par compte** : une copie par compte rendrait la fenêtre de dates fausse (la notification existerait déjà avant la date de début), priverait du message tout compte créé **après** la diffusion, et empêcherait de la modifier ou de la couper d'un coup. **Le message revient à CHAQUE connexion** tant que la période court (choix explicite de l'utilisateur, après une première version qui ne l'affichait qu'une fois). `E_NotificationGlobaleVue` n'est donc **pas** un filtre : elle ne mesure que la **portée** — combien de destinataires distincts l'ont vue au moins une fois. ⚠️ Comme elle est alimentée à chaque connexion, `MarquerVues` **relit d'abord l'existant et n'insère que le manquant** : laisser la contrainte d'unicité lever une exception à chaque connexion coûterait cher pour rien (l'index reste le garde-fou final, deux onglets ouverts en même temps). Pas d'affichage en boucle : `MainLayout` n'interroge les notifications **qu'une fois par session** (garde `_notificationsVerifiees`), donc « à chaque connexion » et non à chaque page. Pour cesser d'afficher avant la date de fin : **désactiver** la diffusion. Admin jamais destinataire. `EstActive` permet de couper une diffusion sans perdre qui l'a vue.
- **Diffusion — un seul point d'accroche** : `S_Notification.ObtenirNonLues` renvoie les notifications personnelles **et** les diffusions actives visées par le rôle de l'appelant ; `MarquerLues` marque les deux. **`MainLayout` n'a donc pas été touché** — il affichait déjà ces toasts, il n'y a pas deux chemins à maintenir. Filtrage des rôles **en mémoire** (les diffusions actives se comptent sur les doigts d'une main ; une recherche de sous-chaîne en base sur une liste concaténée serait fragile). **Liste blanche des rôles ciblables côté serveur**, servie au formulaire par `GET api/notifications-globales/roles` pour que la liste affichée ne diverge jamais de celle qui valide.
- ⚠️ **Fuseaux horaires des diffusions** : PostgreSQL (`timestamp with time zone`) **refuse** un `DateTime` dont le `Kind` est `Unspecified`, et le serveur Railway n'est pas dans le fuseau de l'admin. La conversion en UTC se fait donc **dans le navigateur** (`SpecifyKind(Local).ToUniversalTime()`), où le fuseau est celui de l'admin ; `S_NotificationGlobale.EnUtc` sert de filet sans jamais réinterpréter une heure selon le fuseau du serveur. La **date de fin couvre toute la journée choisie** (23:59:59 local) : l'admin sélectionne un jour, il l'entend inclus.
- **Sidebar conditionnelle** : cachée si non connecté ; menu burger caché aussi
- **Highlight exact** des items menu : `Match="NavLinkMatch.All"`
- **Loader index.html** stylisé : monogramme « K » dans un carré glassmorphism + mot-symbole KEYDO, dégradé turquoise. ⚠️ Écrit en dur dans `index.html` car il s'affiche **avant** le démarrage de Blazor : `Comp_Logo` n'y est pas utilisable.
- **Persistance session** : `Page_Connexion.OnInitializedAsync` redirige si déjà authentifié

## Sources UNIQUES à réutiliser (ne pas re-dupliquer)

**Helpers** — côté client dans `BTPSecure.Client/Services/`, **sauf `H_Siret`** qui vit dans `BTPSecure.Shared/Helpers/` car serveur et client l'utilisent tous les deux :
| Helper | Rôle |
|---|---|
| `H_Siret` *(Shared)* | Nettoyage + validation de la clé de Luhn SIREN/SIRET, hors ligne. Exception La Poste (`356000000`) gérée. |
| `H_CodeParrainage` *(Shared)* | Code d'apporteur : initiales + numéro (`TY-01`), saisie tolérante, garde-fou ASCII. Serveur génère, client valide. |
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
| `Comp_ResultatValidation` | Détail d'une validation (collaborateur, matériaux, PDF), partagé accueil fournisseur + pop-up |
| `Comp_DialogLimiteResponsables` | Réglage admin du plafond Responsable + Responsable Admin |
| `Comp_DialogLimiteSousComptes` | Réglage admin de la limite de sous-comptes d'un fournisseur |
| `Comp_Inactivite` | Déconnexion auto après 10 min d'inactivité (Dirigeant / Responsable / RA) |
| `Comp_PieceJointe` | Affichage + téléchargement d'une pièce jointe de message |
| `Comp_DialogMemo` | Création / édition d'un mémo personnel |
| `Comp_DialogAutoriserParrainage` | Autorisation d'une entreprise parrainée + saisie de la commission (admin) |
| `Comp_DialogSupprimerCompte` | Suppression définitive d'un compte : aperçu des dégâts puis email à recopier |

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

## Points de sauvegarde git
`git tag -l` — tags existants, du plus ancien au plus récent :
`checkpoint-avant-chat`, `checkpoint-avant-charte-keiro`, `checkpoint-phase-test-keydo`, `checkpoint-beta-v2`, `checkpoint-avant-historique`.
Revenir à l'un d'eux : `git reset --hard <tag>` (⚠️ efface les modifications non commitées).

## ⚠️ Limite de vérification locale
**Aucun PostgreSQL sur la machine de dev** (ni Docker, ni `psql`, ni service installé — vérifié). On peut donc valider en local : la compilation, le `publish`, le démarrage du serveur (l'injection de dépendances, les erreurs Npgsql au boot étant normales), les 401 sur les endpoints protégés et le contenu du WASM publié. **Tout ce qui touche la base se valide en prod.** Fonctionnalités livrées dont le parcours complet n'a jamais tourné en local : session unique (2 appareils), historique des codes permanents, notification « commande prête » au destinataire, annuaire de messagerie élargi, **parrainage apporteur d'affaires** (attribution du code, filleul, validation de la commission), **suppression définitive d'un compte** (la cascade de dépendances n'a jamais tourné contre une vraie base — commencer par un compte de test sans historique), **diffusion de notifications** (fenêtre de dates, et réaffichage à chaque connexion).

✅ **`H_CodeParrainage` est en revanche testé** : logique pure, sans base. 44 assertions passées dans un projet isolé (initiales accentuées et turques, saisies à rejeter dont une injection SQL, séquence sans doublon sur 6 attributions, aller-retour `Composer`/`Normaliser`). Reproductible en compilant le seul fichier du helper dans un projet à part (avec `<UseAppHost>false</UseAppHost>`, cf. Smart App Control).

## Commandes utiles
```bash
git log --oneline -5
dotnet publish BTPSecure.Server/BTPSecure.Server.csproj -c Release -o publish
curl -s https://www.keydopro.com/health
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
