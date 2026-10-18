---
name: image-pipeline
description: Conversion et redimensionnement des médias pour le web.
---
- Images : SixLabors.ImageSharp, sortie WebP qualité 80, largeurs 480/800/1200/1920, miniature 400x400.
- Vidéos : ffmpeg, MP4 H.264, CRF 26, image d'aperçu.
- Nommage : `{id}-{largeur}.webp`. Stockage dans `wwwroot/media` (ou stockage externe plus tard). Originaux hors wwwroot.
- Le HTML utilise `<img srcset sizes loading="lazy" alt>` avec largeur/hauteur pour éviter le décalage de mise en page.
