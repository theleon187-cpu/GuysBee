# Plan de réalisation : site vitrine + boutique GuysBee

Statut : **à valider**. Rien n'est lancé tant que ce plan n'est pas approuvé.

## Avancement
- [x] Étape 0 (partielle) : packages ajoutés au `.csproj`, `posts/` et `img/` exclus de la publication, User Secrets activés.
- [x] Étape 1 (code) : entités, `AppDbContext`, `ShippingCalculator`. **Migration non générée** (à faire en validant le schéma).
- [x] Direction éditoriale : `Content/direction-editoriale.md` (concept « De la fleur au pot »).
- [x] Gabarits `Content/analyse-medias.md`, `Content/produits.md` et outil `Tools/AnalyseMedias.cs` (exécuté : 23 fichiers, 18 uniques, tous 1440x1440).
- [x] Données réelles reçues : coordonnées, 4 miels (500 g / 1 kg), logo jaune/noir. Seed : `Data/DbSeeder.cs` (non branché tant qu'il n'y a pas de migration).
- [x] Fondations : Identity (rôle Admin, verrouillage après 5 échecs), initialisation auto de la base (création du schéma tant qu'il n'y a pas de migration), compte admin au premier démarrage, culture fr-FR.
- [x] Pages publiques : Accueil (blocs pilotés par la base), Nos miels, fiche miel, Calendrier de la ruche, Galerie, pages éditables `/p/{slug}`, Où nous trouver, Contact (anti-spam + stockage).
- [x] Admin `/Admin` : connexion, tableau de bord, Photos (import du dossier `posts`, envoi, légendes, publication), Miels et prix, Textes du site (TinyMCE), Messages, Réglages.
- [ ] Reste à faire : migration EF, Livraison (admin + simulateur), panier + PayPal, commandes (admin), journal, menu/accueil/FAQ/lexique/e-mails en admin, utilisateurs, sauvegarde, SEO (sitemap, JSON-LD), cookies, tests.

> Concept du site : voir `Content/direction-editoriale.md`. Le site est centré sur le cycle de la ruche (calendrier des saisons, miel par miellée, journal du rucher), pas sur une vitrine classique.

## 0. Constats sur l'existant
- Solution : `GuysBee.slnx`, un projet Razor Pages `GuysBee` (.NET 10), modèle par défaut (Index, Privacy, Error).
- Dossier `GuysBee/posts/` : **23 images `.webp`** nommées par identifiant Instagram.
- **Aucun fichier de métadonnées trouvé** (ni JSON ni HTML) : pas de légendes, dates ni hashtags.
  - Les légendes et textes seront donc saisis ou rédigés avec l'apiculteur.
  - Si l'export complet existe (`posts_1.json`), il serait utile de l'ajouter.
- Pas de vidéos détectées.
- Le dossier est dans le projet : il faut l'exclure de la compilation et de la publication, et le garder comme source brute.

## 1. Décisions techniques
| Sujet | Choix |
|---|---|
| Framework | Razor Pages, .NET 10 |
| Base de données | EF Core, SQLite en dev, provider à choisir selon l'hébergement |
| Admin | ASP.NET Core Identity, zone `/Admin`, rôle Admin |
| Paiement | PayPal Orders API v2 (sandbox puis live) |
| Images | ImageSharp, WebP en plusieurs largeurs |
| Livraison | Module administrable, calculateur isolé et testé |
| CSS | Additif, préfixe `gb-`, aucun style inline |
| Secrets | User Secrets / variables d'environnement |

## 2. Étapes

### Principe : site vivant, 100 % pilotable depuis l'admin
Objectif : l'apiculteur fait vivre et évoluer le site **sans toucher au code**. Tout ce qui est contenu, réglage ou texte est en base et éditable dans `/Admin`. Le code ne sert qu'aux évolutions de fond.

| Besoin de l'apiculteur | Où dans l'admin | Entité |
|---|---|---|
| Changer les textes des pages (histoire, CGV, mentions) | **Pages** : éditeur visuel, aperçu, publication | `ContentPage` |
| Réorganiser l'accueil (ordre, textes, photos, afficher/masquer) | **Accueil** : blocs glissables | `HomeSection` |
| Modifier le menu et le pied de page | **Menu** | `MenuItem` |
| Ajouter une photo, la légender, la publier | **Médias** : envoi multiple, recadrage, légende, thème, saison | `MediaAsset` |
| Ajouter, modifier un miel, un prix, un stock, « épuisé » | **Produits** | `Product`, `ProductVariant` |
| Publier une actualité | **Journal** | `Post` |
| Décrire les saisons de la ruche | **Calendrier de la ruche** | `HiveCalendarEntry` |
| Expliquer les termes d'apiculture | **Lexique** | `GlossaryTerm` |
| Répondre aux questions fréquentes | **Questions fréquentes** | `FaqItem` |
| Gérer les points de vente | **Où nous trouver** | `SalePoint` |
| Régler la livraison | **Livraison** : assistant + simulateur | `ShippingMethod`... |
| Suivre les commandes, les expédier, imprimer un bon | **Commandes** | `Order` |
| Lire les messages de contact | **Messages** | `ContactMessage` |
| Modifier les e-mails envoyés aux clients | **E-mails** | `EmailTemplate` |
| Coordonnées, réseaux, bandeau d'annonce, couleurs, logo | **Réglages du site** | `SiteSetting` |
| Gérer les comptes (ajouter un utilisateur, réinitialiser un mot de passe) | **Utilisateurs** | Identity |
| Savoir qui a changé quoi | **Journal des modifications** | `AuditEntry` |
| Sauvegarder / restaurer | **Sauvegarde** : export base + médias en un clic | - |

Règles transverses :
- Éditeur de texte visuel : **TinyMCE 7 (décision validée)**, hébergé en local via LibMan (`libman.json`), licence GPL, HTML **assaini** côté serveur (`HtmlCleaner`).
- Aperçu avant publication, statut Brouillon / Publié, date de publication programmée.
- Cache invalidé automatiquement à l'enregistrement.
- Envoi d'images avec conversion WebP automatique.
- Libellés en français, aide contextuelle sur chaque écran, confirmation avant suppression.
- Tableau de bord : commandes à traiter, stocks bas, messages non lus, rappels (ex. « aucune actualité depuis 2 mois »).
- Extensibilité : nouvelles pages créables en admin, nouveaux blocs d'accueil ajoutables sans déploiement.
- Seed : contenu initial pré-rempli (pages légales types, calendrier, FAQ, lexique) à relire par l'apiculteur.

### Étape 0 : Préparation
- Exclure `posts/` de la compilation et de la publication, ajouter les packages, créer l'arborescence de dossiers.
- Configurer les User Secrets (PayPal, SMTP, admin initial).
- **Livrable** : solution qui compile.
- **Validation** : structure des dossiers.

### Étape 1 : Modèle de données
- Entités : Product, ProductVariant, Category, MediaAsset, Post, SalePoint, SiteSetting, Order, OrderLine, Payment, ShippingMethod, ShippingZone, ShippingRate.
- DbContext, migration initiale, seed minimal.
- **Validation** : schéma relu avant création de la migration.

### Étape 2 : Administration
- Identity, connexion, layout admin simple en français.
- Tous les écrans du tableau « Principe : site vivant » ci-dessus, livrés par lots : (a) produits, médias, réglages ; (b) pages, accueil, menu ; (c) journal, calendrier, lexique, FAQ ; (d) messages, e-mails, utilisateurs, journal des modifications, sauvegarde. Les commandes et la livraison suivent aux étapes 5 et 6.
- **Validation** : parcours et libellés, lot par lot.

### Étape 3 : Import des médias
- 3a. Inspection des 23 images (dimensions, doublons, poids) et rapport.
- 3b. Pipeline : WebP en 480/800/1200 px, miniatures, nommage propre. Originaux conservés.
- 3c. Import en brouillon dans `MediaAsset` (aucune légende inventée).
- 3d. Écran admin « Médias » : publier, masquer, ajouter légende, texte alternatif et thème.
- 3e. Si un export JSON est fourni plus tard : parser avec correction d'encodage.
- **Validation** : rapport 3a, avant le code.

### Étape 4 : Pages publiques
- Layout, thème CSS (palette miel/ambre), composants (Hero, ProductCard, Gallery, etc.).
- Pages : Accueil, Notre histoire, Produits, Galerie, Actualités, Où nous trouver, Contact.
- **Validation** : accueil d'abord, puis le reste.

### Étape 5 : Livraison administrable
- Modes : retrait sur place, colis, tournée locale.
- Tarifs : forfait, tranches de poids, franco, par zone/département.
- Délais, jours de préparation, message client, activation en un clic.
- Assistant pas à pas, valeurs par défaut, simulateur de panier.
- Tests unitaires du calculateur.
- **Validation** : scénarios de paniers types.

### Étape 6 : Boutique et PayPal
- Panier (session), commande, choix de livraison, adresse.
- PayPal : création, capture, vérification serveur du montant, webhook signé, idempotence.
- E-mails, statuts de commande, gestion du stock.
- **Validation** : paiement sandbox de bout en bout.

### Étape 7 : SEO, légal, qualité
- Meta, Open Graph, sitemap, robots, JSON-LD (LocalBusiness, Product).
- Cookies, RGPD, CGV, mentions légales (modèles à faire valider).
- Accessibilité, tests, checklist de mise en ligne.

## 3. Règles de travail
- Arrêt et validation à la fin de chaque étape.
- Pas de renommage de classes ou d'ID existants, pas de style inline.
- Aucun contenu inventé (allégations santé, labels bio, etc.) sans confirmation.
- Aucune donnée tierce Instagram publiée.

## 4. Questions ouvertes
1. Existe-t-il un export avec les légendes et dates, ou seulement ces 23 images ?
2. Nom commercial, logo, couleurs : existants, ou une charte est-elle à proposer ?
3. Liste des produits, prix et poids à fournir (ou valeurs fictives pour démarrer) ?
4. Livraison : France uniquement ? Transporteur envisagé ?
5. Importer en second projet console (`GuysBee.Importer`) ou écran dans l'admin ? Recommandation : écran admin, car 23 images ne justifient pas un second projet.
6. Hébergement : toujours à définir. SQLite reste utilisable au démarrage.
