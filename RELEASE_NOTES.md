# Notes de version

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
