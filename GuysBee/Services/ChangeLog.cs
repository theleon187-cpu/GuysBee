namespace GuysBee.Services;

public record ChangeItem(string Category, string Text);

public record ChangeRelease(string Title, DateOnly Date, string Summary, IReadOnlyList<ChangeItem> Items);

/// <summary>Évolutions du site, de la plus récente à la plus ancienne.</summary>
public static class ChangeLog
{
    public const string Public = "Site public";
    public const string Admin = "Administration";
    public const string Shop = "Boutique";
    public const string Tech = "Technique";

    public static readonly IReadOnlyList<ChangeRelease> Releases = new List<ChangeRelease>
    {
        new("Historique et dernières retouches", new DateOnly(2026, 11, 14),
            "Suivi des modifications faites en administration, photos du calendrier et filtre rapide sur « Où nous trouver ».",
            new ChangeItem[]
            {
                new(Admin, "Nouvelle rubrique « Historique » : chaque création, modification ou suppression est conservée avec la date et l'auteur."),
                new(Public, "Calendrier de la ruche : la photo du mois s'affiche aussi sur la page d'accueil."),
                new(Public, "Où nous trouver : filtre rapide des marchés et des commerces, carte plus lisible."),
                new(Tech, "Journal automatique branché sur la base de données (AuditInterceptor), champs sensibles exclus.")
            }),
        new("Photos, recadrage et éditeur de texte", new DateOnly(2026, 11, 14),
            "Les photos se recadrent directement depuis l'administration et les textes s'écrivent avec un éditeur plus complet.",
            new ChangeItem[]
            {
                new(Admin, "Recadrage des photos avant l'envoi (outil de recadrage intégré)."),
                new(Admin, "Éditeur de texte enrichi avec bouton image, aperçu avant publication."),
                new(Admin, "Textes du site, page d'accueil, calendrier, gammes et prestations retouchés pour être plus simples à modifier."),
                new(Public, "Mise en page des textes enrichis harmonisée sur les pages du site."),
                new(Tech, "Nettoyage automatique du contenu saisi (HtmlCleaner, RichText) et import des photos en WebP.")
            }),
        new("Annonce, prestations et menu à plusieurs niveaux", new DateOnly(2026, 11, 14),
            "Nouveaux outils pour mettre en avant une information et organiser le site.",
            new ChangeItem[]
            {
                new(Admin, "Annonce du site : titre, message, bouton, dates de début et de fin, présentation, fermeture mémorisée chez le visiteur."),
                new(Admin, "Prestations : création, ordre d'affichage, photo et tarif indicatif."),
                new(Admin, "Menu du site : titres principaux et sous-liens, réorganisables."),
                new(Public, "Page « Prestations personnalisées » et bandeau d'annonce."),
                new(Public, "Boutique : fiches produits et panier retouchés."),
                new(Tech, "Migration automatique de l'ancien menu vers la nouvelle structure.")
            }),
        new("Mot de passe, galerie et textes", new DateOnly(2026, 11, 14),
            "Sécurité du compte et retouches des pages publiques.",
            new ChangeItem[]
            {
                new(Admin, "Menu « Mot de passe » pour changer son mot de passe."),
                new(Admin, "Page d'accueil : étapes « De la fleur au pot » modifiables."),
                new(Admin, "Agenda des marchés et lieux de revente : placement automatique sur la carte."),
                new(Public, "« Nos miels » devient « Nos produits », « Notre histoire » devient « L'exploitation »."),
                new(Public, "Galerie en mosaïque avec visionneuse (flèches, clavier, glissé du doigt)."),
                new(Public, "Où nous trouver : une seule carte pour tous les marchés et commerces."),
                new(Public, "Contact : téléphone retiré, pied de page avec lien Facebook.")
            }),
        new("Agenda par jours, photos et cartes", new DateOnly(2026, 10, 23),
            "Les rendez-vous deviennent récurrents et les lieux s'illustrent de photos et de cartes.",
            new ChangeItem[]
            {
                new(Admin, "Agenda des marchés par jours de la semaine, avec raccourcis Semaine, Week-end, Tous les jours."),
                new(Admin, "Photo, lien Google Maps et précisions pour les marchés et les lieux de revente."),
                new(Admin, "Choix et import de polices, texte du logo modifiable."),
                new(Public, "Où nous trouver : jours, photo, précisions et lien d'itinéraire."),
                new(Tech, "Services MapLinks, WeekDays et GeoLocator ; nouvelles colonnes créées au démarrage.")
            }),
        new("Paiement, facturation et livraison", new DateOnly(2026, 10, 20),
            "La boutique peut encaisser et expédier.",
            new ChangeItem[]
            {
                new(Shop, "Paiement PayPal configurable depuis l'administration."),
                new(Shop, "Facturation et livraison Colissimo (points relais, numéro de suivi)."),
                new(Admin, "Tableau de bord enrichi.")
            }),
        new("Lancement du site", new DateOnly(2026, 10, 18),
            "Première version complète du site GUY's Bee.",
            new ChangeItem[]
            {
                new(Public, "Pages publiques : accueil, produits, galerie, calendrier de la ruche, où nous trouver, contact."),
                new(Public, "Thème miel : alvéoles et dégradés dorés."),
                new(Shop, "Gammes de produits, panier, commande, espace client."),
                new(Admin, "Administration protégée : textes, photos, produits, commandes, messages, réglages."),
                new(Tech, "Contrôle des liens, limite du carrousel, gestion des erreurs d'import.")
            })
    };

    public static readonly string[] Categories = { Public, Admin, Shop, Tech };
}
