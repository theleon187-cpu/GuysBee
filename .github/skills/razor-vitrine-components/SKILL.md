---
name: razor-vitrine-components
description: Conventions des partials et view components du site.
---
- Composants : Hero, ProductCard, Gallery, SeasonBanner, SalePointCard, NewsCard.
- Un composant = un partial/view component + sa classe CSS dédiée (préfixe `gb-`).
- Données via PageModel ou ViewComponent, jamais d'accès base dans la vue.
- Images toujours via un TagHelper ou partial `_Picture` (srcset, alt, lazy).
