# Direction éditoriale : GUY's Bee

> Document de travail. Les éléments marqués **[À CONFIRMER]** dépendent de la fiche produit, du logo et des photos que je n'ai pas pu lire. Rien n'est inventé : tout fait (labels, chiffres, lieux) doit être validé.

## Idée directrice : « De la fleur au pot »
Le site raconte le **cycle de la ruche** plutôt que de lister des produits. Chaque page invite à suivre le chemin du miel : la fleur, l'abeille, la ruche, la récolte, le pot, votre table.

Le visiteur doit sentir la ruche avant de voir un catalogue.

## Parcours et pages

| Page | Rôle | Idée d'expérience |
|---|---|---|
| **Accueil** | Plonger dans l'univers | Grande photo plein écran, accroche courte, 4 étapes « de la fleur au pot », produits de saison, point de vente le plus proche |
| **La ruche** (Notre histoire) | Faire connaître l'apiculteur | Histoire en quelques lignes, métier, gestes, valeurs **[À CONFIRMER]** |
| **Le calendrier de la ruche** | Contenu original lié à l'apiculture | Frise des saisons : ce que font les abeilles et l'apiculteur chaque mois. Les photos sont rattachées à la saison |
| **Nos miels** (Produits) | Vendre | Fiches par miellée, origine florale, notes de dégustation **[À CONFIRMER]**, conditionnements |
| **Boutique et panier** | Commande | Panier simple, livraison choisie avant paiement, PayPal |
| **Galerie** | Émotion | Mosaïque filtrable par thème (ruches, récolte, produits, paysages) et par saison |
| **Journal de la ruche** (Actualités) | Vie du rucher | Articles courts, un par événement : récolte, mise en pots, marché |
| **Où nous trouver** | Points de vente | Liste et carte, horaires, retrait sur place |
| **Contact** | Échange | Formulaire sécurisé, coordonnées |
| **CGV, mentions légales, confidentialité** | Légal | Modèles à valider |

## Éléments « apicoles » qui distinguent le site
- **Frise des saisons** : un fil visuel qui change selon la période (la page d'accueil met en avant la saison en cours).
- **Alvéoles hexagonales** : motif CSS discret en séparateur, masque des photos de la galerie, fond de cartes produit.
- **Miellée du moment** : bandeau sur les produits disponibles et ceux qui ne le sont plus (miel saisonnier épuisé, retour la saison prochaine).
- **Page par miel** : origine florale, période de récolte, couleur et texture, accords.
- **Lexique de l'apiculteur** : miellée, hausse, extraction, essaim, cire gaufrée. Les termes sont survolables dans les textes.
- **Compte à rebours léger** : « prochaine récolte attendue en… » **[À CONFIRMER]**.
- **Animation sobre** : abeilles qui butinent en SVG léger, respect de `prefers-reduced-motion`.

## Charte visuelle (proposée)
Le logo est une abeille : **jaune dominant et un peu de noir**. La palette s'en inspire (codes exacts du logo à relever **[À CONFIRMER]**).

| Rôle | Proposition |
|---|---|
| Fond | crème chaud `#FFF9E6` |
| Accent principal | jaune abeille `#F5B700` |
| Texte et détails | noir chaud `#1E1A14` |
| Accent secondaire | ambre `#C98A00` |
| Détails | cire claire `#F2E3B8` |

- Typographies : un titre à empattement chaleureux (ex. « Fraunces »), un texte lisible sans empattement (ex. « Inter »). Polices servies localement (RGPD).
- Variables CSS `--gb-*`, classes préfixées `gb-`, aucun style inline.
- Contrastes validés WCAG AA : le jaune sert aux fonds et aux boutons avec **texte noir** (jamais de texte jaune sur fond clair).

## Ton éditorial
- Vouvoiement, phrases courtes, concret, sensoriel (odeurs, textures, saisons).
- Ni jargon ni superlatifs. Pas d'allégation de santé.
- Exemple de ton : « Ce miel est récolté une fois par an, quand la miellée se termine. »
- Légendes des photos : une phrase simple, liée à la saison ou au geste montré.

## Usage des 23 photos
- Classées par **saison** et **thème** à partir de `Content/analyse-medias.md`.
- 1 photo « héros » pour l'accueil, 1 par miel, le reste en galerie et en illustrations du calendrier.
- Les photos avec personnes identifiables ne sont publiées qu'avec accord.
- Format de sortie : WebP en 480/800/1200 px, avec `alt` obligatoire.

## Gestion par l'apiculteur (admin)
Il peut, sans compétence technique : changer les photos, éditer les textes du calendrier, mettre un miel « épuisé », publier une actualité, régler la livraison avec l'assistant.

## À fournir pour finaliser
1. Remplir `Content/analyse-medias.md` et `Content/produits.md`.
2. Couleurs du logo (ou autoriser la palette proposée).
3. Texte de présentation de l'apiculteur (quelques lignes, même brutes).
