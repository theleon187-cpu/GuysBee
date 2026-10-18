---
name: instagram-export-parser
description: Lit un export Instagram (JSON ou HTML) et en extrait médias, légendes et dates.
---
- Repérer `posts_1.json`, `media.json` ou les fichiers HTML dans le dossier d'export.
- Corriger l'encodage : Instagram encode l'UTF-8 en Latin-1 échappé (`\u00c3\u00a9` pour « é »). Réencoder via `Encoding.Latin1.GetBytes` puis `Encoding.UTF8.GetString`.
- Extraire : chemin du média, légende, hashtags, date (timestamp Unix), type.
- Ignorer commentaires, mentions et données d'autres personnes.
- Dédoublonner par hachage du fichier.
- Sortie : `Content/content-draft.json`.
