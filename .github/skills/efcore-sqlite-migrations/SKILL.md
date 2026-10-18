---
name: efcore-sqlite-migrations
description: Conventions EF Core, SQLite et migrations.
---
- `AppDbContext` dans `Data/`, configuration par `IEntityTypeConfiguration<T>`.
- Chaîne de connexion dans `appsettings`, SQLite en dev.
- Décimaux : `HasPrecision(18,2)`. Attention : SQLite stocke les decimal en texte, prévoir un convertisseur si tri/somme en SQL.
- Une migration par fonctionnalité, nommée explicitement. Seed via `HasData` ou un service d'initialisation.
