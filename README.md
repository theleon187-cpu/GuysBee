# GUY's Bee : site vitrine et boutique d'un apiculteur

et sera complété d'une boutique en ligne (panier, commande, comptes clients, paiement PayPal à brancher)

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
- **Tableau de bord** : commandes à préparer et en attente, dernières commandes et messages, état de la boutique (commande en ligne, PayPal, livraison, Colissimo, facturation) et liste « Pour bien démarrer » qui se coche automatiquement.
- **Commandes** : suivi des commandes, statuts et numéros de suivi.
- **Boutique en ligne** : activation de la commande, comptes clients, historique, modes de livraison (retrait sur place, Colissimo à domicile, point relais, tournée locale) avec tarifs par zone ou par tranche de poids, livraison offerte à partir d'un montant, délais et messages.
- **Paiement et livraison** (`/Admin/Integrations`) :
  - **PayPal** : activation, mode test (sandbox) ou réel, identifiant client, clé secrète, devise, nom affiché, test de connexion.
  - **Facturation** : identité du vendeur, SIRET, TVA (non applicable ou taux), préfixe de numérotation, conditions de paiement, message de bas de facture.
  - **Colissimo et points relais** : identifiants Colissimo avec test de connexion, carte de choix du point relais pendant la commande, lien de recherche de repli, adresse d'expédition, poids de l'emballage, poids maximum d'un colis, signature, assurance.
  - Les secrets (clé PayPal, mot de passe Colissimo) sont **chiffrés** (Data Protection) et jamais réaffichés.

### À venir
Branchement du paiement PayPal dans le tunnel de commande (les réglages sont prêts, la commande reste pour l'instant « en attente »), génération des factures et étiquettes Colissimo, e-mails de commande, journal d'actualités, SEO (sitemap, JSON-LD), bandeau cookies. Voir [`PLAN.md`](PLAN.md).

### Production : clés de chiffrement
Les secrets enregistrés depuis l'administration sont chiffrés avec les clés Data Protection d'ASP.NET Core. En production, configurez un stockage durable de ces clés (dossier persistant, base ou coffre) : sans cela, les secrets deviendront illisibles après un redéploiement et il faudra les saisir à nouveau.

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

### Mettre à jour le projet (pull) et envoyer ses changements (push)

```powershell
# Récupérer les dernières modifications
git pull origin main

# Envoyer ses modifications
git add .
git commit -m "Description des changements"
git push origin main
```

Avec Visual Studio : ouvrez `GuysBee.slnx` puis lancez

Avec VS Code : voir la section [Lancer avec VS Code](#lancer-avec-vs-code).

### Lancer avec VS Code

Ce guide est écrit pour les personnes qui ne sont pas développeurs. Il suffit de suivre les étapes dans l'ordre, sans rien modifier dans le code. Comptez environ 20 minutes la première fois, et moins d'une minute ensuite.

> Les textes entre accents graves (`comme ceci`) sont des **commandes** : copiez-les telles quelles dans le terminal, puis appuyez sur **Entrée**.

#### Partie 1 : installer les logiciels (une seule fois)

Installez ces trois programmes avec les options proposées par défaut (bouton « Suivant » jusqu'à la fin) :

1. [Visual Studio Code](https://code.visualstudio.com/) : l'éditeur qui permet de lancer le site.
2. [SDK .NET 10](https://dotnet.microsoft.com/download) : le moteur qui exécute le site. Choisissez « SDK », pas « Runtime ».
3. [Git](https://git-scm.com/downloads) : pour récupérer le projet.

Redémarrez ensuite l'ordinateur.

#### Partie 2 : récupérer et ouvrir le projet (une seule fois)

1. Ouvrez VS Code, puis le menu **Terminal > Nouveau terminal**. Un panneau apparaît en bas de la fenêtre.
2. Choisissez le dossier où placer le projet (par exemple `C:\Sites`) :
   ```powershell
   mkdir C:\Sites
   cd C:\Sites
   ```
3. Récupérez le projet :
   ```powershell
   git clone https://github.com/theleon187-cpu/GuysBee.git
   ```
4. Menu **Fichier > Ouvrir un dossier...**, sélectionnez `C:\Sites\GuysBee`, puis cliquez sur **Oui, je fais confiance aux auteurs**.
   Vous devez voir `GuysBee.slnx` et `README.md` dans la liste de gauche. Sinon, vous avez ouvert le mauvais dossier.
5. En bas à droite, VS Code propose d'**installer les extensions recommandées** : cliquez sur **Installer**. Sinon, ouvrez l'onglet Extensions (`Ctrl+Maj+X`) et installez **C# Dev Kit**. Attendez la fin de l'installation.

#### Partie 3 : préparer le site (une seule fois)

Dans le terminal de VS Code (**Terminal > Nouveau terminal**) :

1. Installez l'outil qui télécharge l'éditeur de texte de l'administration :
   ```powershell
   dotnet tool install -g Microsoft.Web.LibraryManager.Cli
   ```
   Si le message indique que l'outil est « déjà installé », c'est normal.
2. Fermez puis rouvrez le terminal (icône corbeille, puis **Terminal > Nouveau terminal**).
3. Téléchargez l'éditeur : menu **Terminal > Exécuter la tâche...**, choisissez **libman-restore**, puis « Continuer sans analyser la sortie de la tâche ».
4. Autorisez la connexion sécurisée locale (Windows peut demander confirmation : répondez **Oui**) :
   ```powershell
   dotnet dev-certs https --trust
   ```

#### Partie 4 : démarrer le site (à chaque fois)

1. Ouvrez le dossier `GuysBee` dans VS Code.
2. Appuyez sur la touche **F5** (ou menu **Exécuter > Démarrer le débogage**).
3. Si VS Code demande de choisir un environnement, sélectionnez **GuysBee (https/http)**.
4. Patientez : la première fois, la préparation prend 1 à 2 minutes. Le navigateur s'ouvre ensuite sur le site.

Au tout premier démarrage, le site crée lui-même sa base de données et le compte administrateur. Aucune action n'est nécessaire.

- **Site** : `https://localhost:7080`
- **Administration** : `https://localhost:7080/Admin` (identifiants dans la section ci-dessous)

Pour **arrêter** le site : cliquez sur le carré rouge de la barre d'outils en haut de l'écran, ou appuyez sur `Maj+F5`.

#### Un problème ?

| Ce que vous voyez | Ce qu'il faut faire |
|---|---|
| `dotnet` n'est pas reconnu | Le SDK .NET n'est pas installé, ou l'ordinateur n'a pas été redémarré. Refaites la partie 1. |
| `git` n'est pas reconnu | Installez Git, puis fermez et rouvrez VS Code. |
| `libman` n'est pas reconnu | Fermez et rouvrez VS Code, puis relancez la tâche **libman-restore**. |
| Le navigateur affiche « connexion non sécurisée » | Refaites l'étape 4 de la partie 3, puis fermez et rouvrez le navigateur. |
| F5 propose « Installer C# Dev Kit » | Installez l'extension (partie 2, étape 5), attendez la fin, puis réessayez. |
| `Address already in use` (port déjà utilisé) | Le site tourne déjà : arrêtez-le (`Maj+F5`) ou fermez l'autre fenêtre VS Code. |
| L'éditeur de texte de l'administration ne s'affiche pas | La tâche **libman-restore** n'a pas été exécutée (partie 3, étape 3). |
| Les photos ou textes modifiés ont disparu | Ils sont stockés dans le fichier `GuysBee/guysbee.db`. Ne le supprimez pas. |

Pour obtenir de l'aide, copiez le texte rouge affiché dans le terminal et envoyez-le avec une capture d'écran.

Pour les développeurs : rechargement à chaud via **Terminal > Exécuter la tâche > watch**, ou `dotnet run --project GuysBee` sans débogueur.

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
