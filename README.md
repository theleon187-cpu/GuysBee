# GUY's Bee : site vitrine et boutique d'un apiculteur

Site web de **GUY's Bee**, apiculteur à Saint-Julien-sur-Bibost (69). Il présente l'exploitation, les miels et la vie de la ruche, et sera complété d'une boutique en ligne (paiement PayPal) et d'une livraison configurable.

Le site est **administrable sans toucher au code** : textes, photos, prix et stocks se modifient depuis une interface web (`/Admin`).

## Fonctionnalités

### Site public
- **Accueil** : bandeau d'accueil, étapes « de la fleur au pot », miels, ruche du mois (blocs pilotés par la base de données).
- **Nos miels** : miel de printemps, d'acacia, de montagne, de fleurs sauvages (pots de 500 g et 1 kg).
- **Boutique** : produits regroupés par **gamme** (miels, cire, propolis...), barre de gammes et page par gamme (`/Produits?gamme=slug`) avec libellé et introduction.
- **La ruche au fil des saisons**
- **Galerie** photos, **Où nous trouver**, **Contact** (anti-spam intégré).
- **Pages éditables** : histoire, conditions de vente, mentions légales, confidentialité.
- Thème jaune et noir inspiré du logo, accessible (contrastes, navigation clavier, `alt` sur les images), responsive.

### Administration (`/Admin`)
- Connexion sécurisée (ASP.NET Core Identity, verrouillage après 5 échecs).
- **Photos** : import du dossier d'origine, envoi de nouvelles photos, conversion automatique en WebP (480 / 800 / 1200 px), détection des doublons, légendes, thèmes, saisons, publication.
- **Gammes et produits** : création/modification/suppression des gammes (libellé, introduction, ordre), création de produits et de formats (pots), prix, stock, poids d'expédition. Chaque gamme possède un **libellé de menu** : le renseigner crée/renomme automatiquement son lien dans le menu, le vider le retire.
- **Menu du site** : liens du menu principal et du bas de page, choisis dans une **liste déroulante** (pages du site, gammes, pages de contenu), ordre, visibilité.
- **Page d'accueil** : blocs, photos du carrousel (jusqu'à 5, depuis Photos), blocs produits liés à une gamme.
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

Avec VS Code : voir la section [Lancer avec VS Code](#lancer-avec-vs-code).

### Lancer avec VS Code

Section indépendante : elle suffit pour lancer le site sans Visual Studio.

1. Installer le [SDK .NET 10](https://dotnet.microsoft.com/download), [VS Code](https://code.visualstudio.com/) et Git.
2. Cloner le dépôt puis ouvrir **le dossier racine** (celui qui contient `GuysBee.slnx`) : `code .`
3. Accepter les extensions recommandées (**C# Dev Kit**, fichier `.vscode/extensions.json`).
4. Restaurer TinyMCE une seule fois : `dotnet tool install -g Microsoft.Web.LibraryManager.Cli`, puis `Terminal > Exécuter la tâche > libman-restore`.
5. Approuver le certificat HTTPS de développement (une fois) : `dotnet dev-certs https --trust`
6. Appuyer sur **F5** et choisir la configuration « GuysBee (https/http) ». Le projet est compilé (tâche `build`) puis le navigateur s'ouvre.

- Adresses : `https://localhost:7080` et `http://localhost:5080` (administration : `/Admin`).
- Rechargement à chaud : `Terminal > Exécuter la tâche > watch`.
- Sans débogueur : `dotnet run --project GuysBee`.
- La base SQLite `guysbee.db` et le compte administrateur sont créés au premier lancement (voir ci-dessous).

### Connexion à l'administration
Rendez-vous sur `https://localhost:<port>/Admin`.

| Environnement | Identifiant | Mot de passe |
|---|---|---|
| Développement | `admin@guysbee.local` | `guy` (temporaire, à changer) |

> Le mot de passe `guy` n'est accepté qu'en **développement** (`appsettings.Development.json`). En production, une politique stricte s'applique et le mot de passe doit être fourni par configuration (voir ci-dessous). Si la base existe déjà avec un autre administrateur, supprimez `guysbee.db` pour la recréer.

### Photos et logo
`GuysBee/wwwroot/img/logo.jpg`
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
Tant qu'aucune migration n'existe, le schéma est créé automatiquement et les colonnes ajoutées ensuite (`MediaAssets.ShowInHero`, `Categories.MenuLabel`) le sont par `ALTER TABLE` au démarrage (`DbInitializer`). Pour passer aux migrations :

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
