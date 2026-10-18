namespace GuysBee.Data.Entities;

/// <summary>
/// Page éditable (Notre histoire, CGV, mentions légales...). Le contenu est géré depuis l'admin.
/// </summary>
public class ContentPage
{
    public int Id { get; set; }
    /// <summary>Clé technique stable : "histoire", "cgv", "mentions-legales", "confidentialite".</summary>
    public string Key { get; set; } = "";
    public string Title { get; set; } = "";
    public string Slug { get; set; } = "";
    /// <summary>Contenu en HTML assaini (éditeur visuel dans l'admin).</summary>
    public string BodyHtml { get; set; } = "";
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public int? HeroMediaId { get; set; }
    public MediaAsset? HeroMedia { get; set; }
    public bool IsPublished { get; set; } = true;
    public bool ShowInMenu { get; set; }
    public bool ShowInFooter { get; set; }
    public int SortOrder { get; set; }
    public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Bloc de la page d'accueil : l'apiculteur choisit l'ordre, le texte, l'image et l'affichage.
/// </summary>
public class HomeSection
{
    public int Id { get; set; }
    /// <summary>Type : Hero, Etapes, ProduitsDeSaison, Calendrier, Actualites, PointsDeVente, Texte.</summary>
    public string Kind { get; set; } = "Texte";
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public string? BodyHtml { get; set; }
    public string? ButtonLabel { get; set; }
    public string? ButtonUrl { get; set; }
    public int? MediaId { get; set; }
    public MediaAsset? Media { get; set; }
    public bool IsVisible { get; set; } = true;
    public int SortOrder { get; set; }
}

/// <summary>Entrée du menu de navigation, modifiable en admin.</summary>
public class MenuItem
{
    public int Id { get; set; }
    public string Label { get; set; } = "";
    public string Url { get; set; } = "";
    public bool IsVisible { get; set; } = true;
    public bool IsFooter { get; set; }
    public int SortOrder { get; set; }
}

/// <summary>Étape du calendrier de la ruche (une par mois ou par saison).</summary>
public class HiveCalendarEntry
{
    public int Id { get; set; }
    /// <summary>1 à 12.</summary>
    public int Month { get; set; }
    public string Title { get; set; } = "";
    public string WhatBeesDo { get; set; } = "";
    public string WhatBeekeeperDoes { get; set; } = "";
    public int? MediaId { get; set; }
    public MediaAsset? Media { get; set; }
    public bool IsVisible { get; set; } = true;
}

/// <summary>Terme du lexique apicole (survol dans les textes).</summary>
public class GlossaryTerm
{
    public int Id { get; set; }
    public string Term { get; set; } = "";
    public string Definition { get; set; } = "";
}

/// <summary>Question fréquente (livraison, conservation du miel, cristallisation...).</summary>
public class FaqItem
{
    public int Id { get; set; }
    public string Question { get; set; } = "";
    public string AnswerHtml { get; set; } = "";
    public bool IsVisible { get; set; } = true;
    public int SortOrder { get; set; }
}

/// <summary>Message envoyé par le formulaire de contact, consultable et traitable en admin.</summary>
public class ContactMessage
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Phone { get; set; }
    public string Subject { get; set; } = "";
    public string Body { get; set; } = "";
    public bool IsRead { get; set; }
    public bool IsArchived { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}

/// <summary>Texte d'e-mail modifiable (confirmation de commande, expédition...).</summary>
public class EmailTemplate
{
    public int Id { get; set; }
    /// <summary>OrderConfirmation, OrderShipped, ContactReceived...</summary>
    public string Key { get; set; } = "";
    public string Subject { get; set; } = "";
    public string BodyHtml { get; set; } = "";
}

/// <summary>Trace des actions faites en admin (qui a modifié quoi).</summary>
public class AuditEntry
{
    public int Id { get; set; }
    public string UserName { get; set; } = "";
    public string Action { get; set; } = "";
    public string EntityName { get; set; } = "";
    public string? EntityId { get; set; }
    public string? Details { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
