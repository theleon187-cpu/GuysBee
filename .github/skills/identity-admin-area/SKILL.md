---
name: identity-admin-area
description: Zone d'administration Razor Pages protégée par Identity.
---
- Area `Admin` avec convention `AuthorizeAreaFolder("Admin", "/", "AdminOnly")`.
- Création du premier administrateur au démarrage depuis la configuration (secret), mot de passe à changer à la première connexion.
- Layout admin séparé, simple, grands boutons, messages de confirmation en français.
- Téléversement de fichiers : vérifier extension, type MIME, signature et taille.
