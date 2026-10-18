# GUY's Bee : site vitrine et boutique d'un apiculteur

Site web de **GUY's Bee**, apiculteur à Saint-Julien-sur-Bibost (69). Il présente l'exploitation, les miels et la vie de la ruche, et sera complété d'une boutique en ligne (paiement PayPal) et d'une livraison configurable.

Le site est **administrable sans toucher au code** : textes, photos, prix et stocks se modifient depuis une interface web (`/Admin`).

## Fonctionnalités

### Site public
- **Accueil** : bandeau d'accueil, étapes « de la fleur au pot », miels, ruche du mois (blocs pilotés par la base de données).
- **Nos miels** : miel de printemps, d'acacia, de montagne, de fleurs sauvages (pots de 500 g et 1 kg).
- **La ruche au fil des saisons** : calendrier mensuel de la vie des abeilles et de l'apiculteur.
- **Galerie** photos, **Où nous trouver**, **Contact** (anti-spam intégré).
- **Pages éditables** : histoire, conditions de vente, mentions légales, confidentialité.
- Thème jaune et noir inspiré du logo, accessible (contrastes, navigation clavier, `alt` sur les images), responsive.

### Administration (`/Admin`)
- Connexion sécurisée (ASP.NET Core Identity, verrouillage après 5 échecs).
- **Photos** : import du dossier d'origine, envoi de nouvelles photos, conversion automatique en WebP (480 / 800 / 1200 px), détection des doublons, légendes, thèmes, saisons, publication.
- **Miels et prix** : textes, prix, stock, poids d'expédition.
- **Textes du site** avec éditeur visuel (TinyMCE), HTML assaini côté serveur.
- **Messages** reçus par le formulaire de contact, **Réglages** (coordonnées, bandeau d'annonce).

### À venir
Livraison administrable (retrait, colis, tournée locale), panier et paiement PayPal, suivi des commandes, journal d'actualités, SEO (sitemap, JSON-LD), bandeau cookies. Voir [`PLAN.md`](PLAN.md).

## Technologies
- .NET 10, **ASP.NET Core Razor Pages**
- Entity Framework Core + **SQLite** (développement)
- ASP.NET Core Identity
- SixLabors.ImageSharp (images), HtmlSanitizer, TinyMCE (via LibMan)

## Installation en local

### Prérequis
- [SDK .NET 10](https://dotnet.microsoft.com/download)
- Visual Studio 2026 (ou `dotnet` en ligne de commande)
- Git

### Étapes

```powershell
# 1. Récupérer le projet
git clone https://github.com/theleon187-cpu/GuysBee.git
cd GuysBee

# 2. Restaurer TinyMCE (éditeur visuel de l'admin)
dotnet tool install -g Microsoft.Web.LibraryManager.Cli
cd GuysBee
libman restore
cd ..

# 3. (Facultatif) Ajouter les photos et le logo, voir plus bas

# 4. Lancer le site
dotnet run --project GuysBee
```

Au premier lancement, le site :
1. crée la base SQLite `guysbee.db` ;
2. insère les données de départ (miels, prix, pages, calendrier, FAQ, menu) ;
3. crée le compte administrateur.

TinyMCE n'est pas restauré automatiquement au build : lancez `libman restore` après un clone (dans Visual Studio : clic droit sur `libman.json` > Restaurer les bibliothèques côté client).

Avec Visual Studio : ouvrez `GuysBee.slnx` puis lancez le projet `GuysBee` (F5).

Avec VS Code : ouvrez le dossier à la racine, installez l'extension **C# Dev Kit**, puis F5 (configuration « GuysBee (https/http) », fichiers `.vscode/launch.json` et `.vscode/tasks.json`). Le site écoute sur `https://localhost:7080` et `http://localhost:5080`. La tâche `watch` offre le rechargement à chaud. Si besoin : `dotnet dev-certs https --trust`.

### Connexion à l'administration
Rendez-vous sur `https://localhost:<port>/Admin`.

| Environnement | Identifiant | Mot de passe |
|---|---|---|
| Développement | `admin@guysbee.local` | `guy` (temporaire, à changer) |

> Le mot de passe `guy` n'est accepté qu'en **développement** (`appsettings.Development.json`). En production, une politique stricte s'applique et le mot de passe doit être fourni par configuration (voir ci-dessous). Si la base existe déjà avec un autre administrateur, supprimez `guysbee.db` pour la recréer.

### Photos et logo
- **Logo** : placez-le dans `GuysBee/wwwroot/img/logo.png`.
- **Photos d'origine** : placez-les dans `GuysBee/posts/` (ce dossier n'est pas versionné), puis dans l'administration : **Photos > Importer le dossier des photos**. Les doublons sont ignorés. Les photos arrivent en brouillon : ajoutez une description puis publiez-les.
- Un outil facultatif analyse les photos : `dotnet run Tools/AnalyseMedias.cs` (rapport dans `Content/analyse-techniques.md`).

### Configuration
| Clé | Rôle |
|---|---|
| `ConnectionStrings:Default` | Chaîne de connexion (défaut : `Data Source=guysbee.db`) |
| `Admin:Email` / `Admin:Password` | Compte administrateur créé au premier démarrage |
| `Media:SourceFolder` | Dossier des photos à importer (défaut : `GuysBee/posts`) |

Pour ne pas stocker de secret dans le dépôt, utilisez les User Secrets :

```powershell
cd GuysBee
dotnet user-secrets set "Admin:Email" "vous@exemple.fr"
dotnet user-secrets set "Admin:Password" "UnMotDePasseSolide!"
```

Si aucun mot de passe n'est configuré en production, un mot de passe aléatoire est généré et affiché une seule fois dans les journaux.

### Migrations de base de données
Tant qu'aucune migration n'existe, le schéma est créé automatiquement. Pour passer aux migrations :

```powershell
dotnet tool install --global dotnet-ef
dotnet ef migrations add Initial --project GuysBee
```

## Structure du projet

```
GuysBee/
├─ GuysBee/                  Projet web Razor Pages
│  ├─ Areas/Admin/           Administration
│  ├─ Components/            View components (menu)
│  ├─ Data/                  DbContext, entités, données de départ
│  ├─ Pages/                 Pages publiques
│  ├─ Services/              Images, assainissement HTML, calcul de livraison
│  └─ wwwroot/               CSS, JS, médias générés
├─ Content/                  Notes éditoriales, analyse des médias, produits
├─ Tools/                    Outils (analyse des photos)
├─ .github/                  Instructions Copilot, agents, skills
└─ PLAN.md                   Plan de réalisation et avancement
```

## Contribuer
Le plan, les décisions et l'avancement sont dans [`PLAN.md`](PLAN.md). Conventions : Razor Pages uniquement, CSS additif avec classes préfixées `gb-`, aucun style inline, secrets hors dépôt.

## Licence
Projet privé pour GUY's Bee. Tous droits réservés. Les photos et le logo appartiennent à leur propriétaire.
