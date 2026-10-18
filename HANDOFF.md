# HANDOFF : état du projet GUY's Bee

Document de reprise pour un nouveau contexte. Dernière mise à jour : fin de session.
Voir aussi `PLAN.md` (feuille de route), `README.md` (installation), `Content/` (direction éditoriale, analyse des médias, produits).

## 1. Objectif
Site apicole **GUY's Bee** (69690 Saint-Julien-sur-Bibost) : pas un site vitrine classique, centré sur l'apiculture et les médias, avec vente en ligne (PayPal) et points de vente. Administrable via l'interface web (`/Admin`) sans toucher au code, et évolutif.
- Stack : .NET 10, **Razor Pages uniquement** (pas de Blazor/MVC), EF Core + SQLite, ASP.NET Core Identity.
- Contraintes : CSS additif, jamais de style inline, ne jamais renommer de classe/ID existant, français, secrets via User Secrets.
- Dépôt : https://github.com/theleon187-cpu/GuysBee (branche `main`).

## 2. Contact et marque
GUY's Bee, 69690 Saint-Julien-sur-Bibost, 06 50 51 12 36, GUYsBee@hotmail.com, Instagram GUYs.Bee, Facebook GUY'S Bee. Palette : #F5B700, #1E1A14, #FFF9E6. Logo : `GuysBee/wwwroot/img/logo.jpg`.

## 3. Ce qui est fait
- **Médias** : 23 WebP 1440x1440 dans `GuysBee/posts/` (5 doublons exacts, 18 uniques) + 1 mp4. Aucun JSON/HTML Instagram (donc pas de légendes ni de dates).
- **Modèle EF Core** (`Data/`) : catalogue, médias, pages, sections d'accueil, menu, calendrier de la ruche, FAQ, lexique, contacts, e-mails, audit, commandes, livraison (retrait, colis, tournée ; zones, tarifs par poids, franco).
- **Seed** : 4 miels réels (printemps 9,10/16,80 ; acacia 11,20/21,00 ; montagne 10,50/19,60 ; fleurs sauvages 9,80/18,20 ; pots 500 g / 1 kg), stock 0 par défaut, poids d'expédition estimés. Textes légaux = modèles à valider. Pages, menu, calendrier 12 mois, FAQ.
- **Identity** : rôle Admin, policy `AdminOnly`, cookie `/Admin/Login`, verrouillage après 5 échecs. Dev : `admin@guysbee.local` / `guy` (`appsettings.Development.json`, politique assouplie en dev uniquement).
- **Pages publiques** : accueil par blocs, Produits (liste/détail), Calendrier, Galerie, Contenu `/p/{slug}`, Où nous trouver, Contact (honeypot).
- **Admin** : Photos (import dossier, upload, WebP 480/800/1200, dédoublonnage SHA-256), Miels (prix/stock/poids), Textes (TinyMCE), Messages, Réglages.
- **Services** : `MediaService` (ImageSharp), `HtmlCleaner` (HtmlSanitizer), `DbInitializer`, `ShippingCalculator` (non testé).
- **Outillage** :

## 4. Correctifs de cette session
1. `GuysBee.csproj` : éléments `None Include` dupliqués remplacés par `None Update` ; commentaire sans accent (encodage) ; fichier recréé.
2. LibMan : fournisseur `unpkg` remplacé par `jsdelivr` (erreur « tinymce@7.6.0 non résolue ») ; package `Microsoft.Web.LibraryManager.Build` retiré (restauration manuelle : `libman restore`). TinyMCE est déjà présent dans `wwwroot/lib/tinymce/`.
3. `.gitignore` : `.vscode/*` ignoré sauf `launch.json` et `tasks.json`.
4. README : section VS Code, restauration TinyMCE manuelle, logo en `.jpg`.
5. Logo : `_SiteLayout.cshtml` pointait vers `logo.png` inexistant ; corrigé en `logo.jpg` (affiché en 48x48, à ajuster si non carré).

## 4bis. Gammes de produits et menu (session récente)
- `Category` : `Name` (libellé public), `Slug` (URL), `Description` (introduction), `MenuLabel` (libellé du lien de menu, nul = pas de lien), `SortOrder`.
- Boutique `/Produits` : barre de gammes si plusieurs gammes utilisées, regroupement par gamme, filtre `?gamme=slug` (404 si inconnue). Cartes dans `Pages/Produits/_ProductCard.cshtml`.
- Admin `/Admin/Miels` (« Gammes et produits ») : ajout, édition, ordre, suppression (refusée si produits rattachés) des gammes ; `SyncMenuAsync` crée/renomme/supprime le `MenuItem` `/Produits?gamme=slug` selon `MenuLabel`.
- Admin `/Admin/Navigation` : l'adresse de chaque lien est une liste déroulante groupée (pages du site, gammes, pages de contenu) ; une adresse inconnue reste proposée (« Autre : »). Plus de saisie d'URL externe.
- Admin Photos : jusqu'à 5 photos du carrousel (`MediaAsset.ShowInHero`) ; Accueil : blocs produits liés à une gamme.
- `DbInitializer` ajoute `Categories.MenuLabel` et `MediaAssets.ShowInHero` par `ALTER TABLE` tant qu'il n'y a pas de migration.
- Build vérifié (réussi) après ces changements ; pages non testées dans un navigateur. Seule la gamme « Nos miels » (slug `miels`) existe au départ.

## 5. Limites de l'agent et points non vérifiés
- Aucun terminal ni outil Git : **aucun build, aucun test, aucun `git push` n'a été exécuté par l'agent**. Rien n'est validé par compilation.
- Le dernier état du build est inconnu : si erreurs, les coller (code, fichier, ligne).
- Risques de build suspectés : versions flottantes des packages, syntaxe Razor (attributs booléens checked/selected), ViewComponent Menu, layouts.
- `guysbee.db`, `-shm`, `-wal` apparaissent dans le projet : vérifier qu'ils sont ignorés par Git.

## 6. À faire (ordre conseillé)
1. `dotnet build GuysBee/GuysBee.csproj` et corriger les erreurs.
2. Publier : `git add . ; git commit -m "..." ; git push origin main` (si refus : `git pull --rebase origin main`).
3. Générer la migration EF (`dotnet ef migrations add Initial --project GuysBee`) ; `EnsureCreated` n'est qu'un repli.
4. Importer les photos via Admin > Photos, rédiger légendes et publier.
5. Admin manquant : calendrier/FAQ/lexique, e-mails, utilisateurs, audit, sauvegarde (produits et menu sont désormais gérables). Tester les gammes dans le navigateur en créant une 2e gamme (ex. Cire).
6. Livraison administrable (assistant, aperçu panier test), panier, paiement PayPal (Orders API v2), commandes, e-mails.
7. SEO (meta, Open Graph, sitemap, JSON-LD LocalBusiness/Product), RGPD, bandeau cookies, journal d'actualités.
8. Tests (`ShippingCalculator`, import médias), checklist de mise en ligne.
9. Compléter les informations manquantes : mentions légales (SIRET, statut), origines des miels, stock réel, tarifs de livraison réels.

## 7. Commandes utiles
```powershell
dotnet build GuysBee/GuysBee.csproj
dotnet run --project GuysBee
cd GuysBee ; libman restore          # TinyMCE
dotnet dev-certs https --trust
```
Réinitialiser la base en dev : supprimer `GuysBee/guysbee.db*`.
