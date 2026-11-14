# Notes de version

## Retouches et nouveautés (carte unique, prestations, galerie)

### Site public
- « Nos miels » devient « Nos produits », « Notre histoire » devient « L'exploitation ».
- Logo agrandi, sans rond orange ; nom du site plus grand.
- Nouvelle page « Prestations personnalisées » (menu « Prestations »), modifiable dans Textes du site.
- Galerie : mosaïque respectant le format des photos, visionneuse avec photo précédente/suivante (flèches, clavier, glissé du doigt), légende et compteur ; invitation à suivre Instagram et Facebook.
- Où nous trouver : une seule carte (OpenStreetMap) avec tous les marchés (orange) et commerces (vert) ; les cartes individuelles disparaissent, la page est bien plus rapide.
- Calendrier de la ruche : une photo par mois.
- Contact : téléphone et mention de visite de la miellerie retirés.
- Pied de page : lien Facebook.

### Administration
- Réglages : adresse de la page Facebook.
- Page d'accueil : les étapes « De la fleur au pot » sont modifiables (une ligne par étape : `Titre | texte`).
- Textes du site : bouton image dans l'éditeur pour insérer des photos dans les pages.
- Calendrier de la ruche : ajout/retrait d'une photo par mois.
- Marchés et lieux de revente : placement automatique sur la carte à l'enregistrement (lien Google Maps ou adresse), bouton « Placer sur la carte les … manquants ».
- Nouveau menu « Mot de passe » pour changer son mot de passe.

### Technique
- Colonnes ajoutées au démarrage : `SalePoints.Latitude/Longitude`, `MarketEvents.Latitude/Longitude`.
- Service `GeoLocator` (coordonnées du lien Google ou recherche OpenStreetMap Nominatim, uniquement côté admin).
- Leaflet 1.9.4 chargé seulement sur « Où nous trouver » ; script `wwwroot/js/points-map.js`.

### À savoir
- Après mise à jour, ouvrir Admin > Agenda des marchés et Lieux de revente puis cliquer sur « Placer sur la carte… » (5 lieux par clic).

## Mise en ligne (hébergement)
1. Choisir un hébergeur compatible ASP.NET Core .NET 10 (Azure App Service, VPS Linux/Windows chez OVH, Infomaniak, etc.). Un hébergement mutualisé « PHP » classique ne convient pas.
2. Publier : `dotnet publish GuysBee -c Release -o publish`.
3. Copier le dossier `publish` sur le serveur, avec le dossier `wwwroot/media` et la base `guysbee.db` (à sauvegarder régulièrement).
4. Renseigner les secrets en variables d'environnement : PayPal, SMTP, mot de passe admin initial.
5. Activer HTTPS (certificat Let's Encrypt) et rattacher le nom de domaine.
6. Après la première connexion, changer le mot de passe dans Admin > Mot de passe.

## Agenda et lieux de revente

### Agenda des marchés (Admin/Marches)
- Les rendez-vous se programment par jours de la semaine (récurrents) au lieu d'une date précise.
- Sélection de plusieurs jours en un clic (pastilles Lun à Dim) avec raccourcis : Semaine, Week-end, Tous les jours, Effacer.
- Les rendez-vous existants sont convertis automatiquement vers le nouveau système.
- Ajout d'une photo et d'un lien Google Maps.

### Lieux de revente (Admin/Revendeurs)
- Ajout d'une photo, d'un lien Google Maps et d'un champ « Précisions ».

### Page « Où nous trouver »
- Affichage des jours, de la photo et des précisions.
- Lien « Itinéraire sur Google Maps » et carte intégrée affichée par défaut lorsque le lien est renseigné.

### Technique
- Nouvelles colonnes ajoutées au démarrage : `MarketEvents.DaysMask`, `WeekDay`, `MapUrl`, `PhotoBaseName` ; `SalePoints.MapUrl`, `PhotoBaseName`, `Details`.
- Nouveaux services : `MapLinks` (validation des liens Google, URL de carte), `WeekDays` (jours en masque de bits).
- `MediaService.SavePhotoAsync` : import d'une photo en WebP (480/800/1200 px), réutilisée si déjà présente.
- Le style du sélecteur de jours est dans `admin.css`.

### À savoir
- Redémarrer l'application pour créer les colonnes en base.
- Les photos envoyées sont enregistrées en brouillon dans la médiathèque (non visibles dans la galerie).
