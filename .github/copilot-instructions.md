# Contexte
Site vitrine + boutique pour un apiculteur (projet GuysBee, .NET 10, Razor Pages).
Source de contenu : export Instagram dans `C:\Dev\GuyBee\posts` (hors solution, lu uniquement par l'outil d'import).

# Objectifs
- Présenter l'exploitation, les produits (miels, cire, propolis...), le savoir-faire, la galerie, les points de vente, le contact.
- Vendre en ligne (paiement PayPal) et proposer des points de vente physiques.
- Site administrable par un non-informaticien (back-office simple, vocabulaire métier, aucun jargon technique).
- Ton chaleureux, authentique, artisanal. Langue : français.
- Mobile first, rapide, accessible (WCAG AA), SEO local.

# Contraintes techniques
- Razor Pages uniquement (pas de Blazor, pas de MVC).
- EF Core (SQLite en dev, base finale selon hébergement), ASP.NET Core Identity pour l'admin (zone /Admin).
- CSS additif, classes dédiées, jamais de style inline. Ne jamais renommer de classe ou d'ID existant.
- Images optimisées (WebP, srcset, lazy loading, alt obligatoire).
- Aucune donnée tierce Instagram (commentaires, abonnés) n'est importée ni publiée.
- Secrets (PayPal, SMTP) via User Secrets / variables d'environnement, jamais dans le dépôt.
- Pas de dépendance lourde sans justification.

# Livraison (module administrable)
- Modes de livraison configurables en admin : retrait sur place, colis (transporteur), tournée locale.
- Tarifs par mode : forfait, par tranche de poids, gratuit au-dessus d'un montant, par zone/département.
- Délais, jours de préparation, message affiché au client, activation/désactivation en un clic.
- Assistant de configuration pas à pas, valeurs par défaut pré-remplies, aperçu du calcul pour un panier test.

# Livrables
1. Arborescence : Accueil, Notre histoire, Produits, Boutique/Panier, Galerie, Actualités, Où nous trouver, Contact, CGV, Mentions légales.
2. Outil d'import Instagram (GuysBee.Importer).
3. Modèle de données, migrations, seed.
4. Back-office /Admin.
5. Boutique + paiement PayPal + e-mails.
6. SEO (meta, Open Graph, sitemap, JSON-LD LocalBusiness/Product), RGPD, cookies.
7. Tests et checklist de mise en ligne.

# Méthode
Travailler par étapes, valider chacune avant la suivante. Poser des questions si une information manque.
