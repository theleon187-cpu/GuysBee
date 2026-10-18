using GuysBee.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Data;

/// <summary>
/// Contenu initial éditable : pages, calendrier, FAQ, menu. Tout est modifiable ensuite dans l'admin.
/// Les textes sont des bases à relire et valider par l'apiculteur.
/// </summary>
public static class ContentSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (!await db.MenuItems.AnyAsync())
        {
            db.MenuItems.AddRange(
                new MenuItem { Label = "Accueil", Url = "/", SortOrder = 1 },
                new MenuItem { Label = "Nos miels", Url = "/Produits", SortOrder = 2 },
                new MenuItem { Label = "La ruche au fil des saisons", Url = "/Calendrier", SortOrder = 3 },
                new MenuItem { Label = "Galerie", Url = "/Galerie", SortOrder = 4 },
                new MenuItem { Label = "Notre histoire", Url = "/p/histoire", SortOrder = 5 },
                new MenuItem { Label = "Où nous trouver", Url = "/OuNousTrouver", SortOrder = 6 },
                new MenuItem { Label = "Contact", Url = "/Contact", SortOrder = 7 },
                new MenuItem { Label = "Conditions de vente", Url = "/p/cgv", IsFooter = true, SortOrder = 1 },
                new MenuItem { Label = "Mentions légales", Url = "/p/mentions-legales", IsFooter = true, SortOrder = 2 },
                new MenuItem { Label = "Confidentialité", Url = "/p/confidentialite", IsFooter = true, SortOrder = 3 });
        }

        if (!await db.ContentPages.AnyAsync())
        {
            db.ContentPages.AddRange(
                new ContentPage
                {
                    Key = "histoire", Slug = "histoire", Title = "Notre histoire", SortOrder = 1,
                    MetaDescription = "L'histoire de GUY's Bee, apiculteur à Saint-Julien-sur-Bibost.",
                    BodyHtml = "<p>Ici, l'apiculteur racontera son histoire : comment il a commencé, ses ruches, ses lieux de butinage et sa façon de travailler.</p><p><em>Texte à rédiger dans l'administration, rubrique « Pages ».</em></p>"
                },
                new ContentPage
                {
                    Key = "cgv", Slug = "cgv", Title = "Conditions générales de vente", SortOrder = 2,
                    BodyHtml = "<p><strong>Modèle à faire valider avant la mise en ligne.</strong></p><h2>Vendeur</h2><p>GUY's Bee, 69690 Saint-Julien-sur-Bibost. Téléphone : 06 50 51 12 36. E-mail : GUYsBee@hotmail.com.</p><h2>Produits et prix</h2><p>Les prix sont indiqués en euros, toutes taxes comprises. Ils peuvent évoluer ; le prix applicable est celui affiché au moment de la commande.</p><h2>Paiement</h2><p>Le paiement s'effectue en ligne via PayPal.</p><h2>Livraison</h2><p>Les modes, délais et frais de livraison sont indiqués avant la validation de la commande.</p><h2>Droit de rétractation</h2><p>À compléter selon la réglementation en vigueur.</p>"
                },
                new ContentPage
                {
                    Key = "mentions-legales", Slug = "mentions-legales", Title = "Mentions légales", SortOrder = 3,
                    BodyHtml = "<p><strong>Modèle à compléter.</strong></p><h2>Éditeur du site</h2><p>GUY's Bee, 69690 Saint-Julien-sur-Bibost. Téléphone : 06 50 51 12 36. E-mail : GUYsBee@hotmail.com.</p><p>SIRET : à compléter. Numéro d'apiculteur (NAPI) : à compléter.</p><h2>Hébergeur</h2><p>À compléter.</p>"
                },
                new ContentPage
                {
                    Key = "confidentialite", Slug = "confidentialite", Title = "Politique de confidentialité", SortOrder = 4,
                    BodyHtml = "<p><strong>Modèle à faire valider.</strong></p><p>Les données saisies (commande, formulaire de contact) servent uniquement à traiter votre demande. Elles ne sont ni revendues ni cédées. Pour exercer vos droits, écrivez à GUYsBee@hotmail.com.</p>"
                });
        }

        if (!await db.HiveCalendarEntries.AnyAsync())
        {
            (int M, string T, string Bees, string Keeper)[] months =
            [
                (1, "Janvier : le repos", "Les abeilles forment une grappe pour se réchauffer autour de la reine.", "L'apiculteur laisse les colonies au calme et surveille les ruches de loin."),
                (2, "Février : le réveil discret", "La reine recommence à pondre, la colonie consomme ses réserves.", "Vérification des réserves de nourriture, sans ouvrir les ruches."),
                (3, "Mars : premières sorties", "Les premiers pollens arrivent, les abeilles sortent par beau temps.", "Premières visites, contrôle de l'état des colonies."),
                (4, "Avril : l'essor du printemps", "La colonie grandit vite, les floraisons se multiplient.", "Pose des hausses pour laisser de la place à la future récolte."),
                (5, "Mai : la grande miellée", "Les butineuses rapportent du nectar en abondance.", "Surveillance de l'essaimage, première récolte possible selon les floraisons."),
                (6, "Juin : récolte de printemps", "Les abeilles continuent leur travail de butinage.", "Récolte et mise en pots des miels de printemps."),
                (7, "Juillet : l'été en altitude", "Les colonies profitent des floraisons de l'été.", "Selon les années, récolte de miels d'été ou de montagne."),
                (8, "Août : la fin des récoltes", "La ponte ralentit, les abeilles préparent l'automne.", "Dernières récoltes, bilan des ruches."),
                (9, "Septembre : préparer l'hiver", "Les abeilles d'hiver naissent, plus robustes.", "Soins et nourrissement si nécessaire."),
                (10, "Octobre : les ruches se resserrent", "La colonie se réduit et se regroupe.", "Protection des ruches contre le froid et les visiteurs."),
                (11, "Novembre : le calme", "Les sorties se font rares.", "Entretien du matériel, étiquetage et préparation des commandes."),
                (12, "Décembre : l'hiver", "La grappe d'hiver se resserre.", "L'apiculteur laisse faire la nature.")
            ];
            foreach (var m in months)
                db.HiveCalendarEntries.Add(new HiveCalendarEntry { Month = m.M, Title = m.T, WhatBeesDo = m.Bees, WhatBeekeeperDoes = m.Keeper });
        }

        if (!await db.FaqItems.AnyAsync())
        {
            db.FaqItems.AddRange(
                new FaqItem { Question = "Mon miel est devenu dur, est-il abîmé ?", AnswerHtml = "<p>Non. La cristallisation est un phénomène naturel. Pour le rendre liquide, placez le pot dans un bain-marie tiède (moins de 40 °C).</p>", SortOrder = 1 },
                new FaqItem { Question = "Comment conserver le miel ?", AnswerHtml = "<p>À l'abri de la lumière et de la chaleur, pot fermé, à température ambiante.</p>", SortOrder = 2 },
                new FaqItem { Question = "Pouvez-vous m'expédier ma commande ?", AnswerHtml = "<p>Les modes de livraison disponibles sont affichés lors de la commande.</p>", SortOrder = 3 });
        }

        if (!await db.HomeSections.AnyAsync())
        {
            db.HomeSections.AddRange(
                new HomeSection { Kind = "Hero", Title = "Du miel récolté avec soin", Subtitle = "À Saint-Julien-sur-Bibost, GUY's Bee veille sur ses abeilles de la fleur au pot.", ButtonLabel = "Découvrir nos miels", ButtonUrl = "/Produits", SortOrder = 1 },
                new HomeSection { Kind = "Etapes", Title = "De la fleur au pot", SortOrder = 2 },
                new HomeSection { Kind = "ProduitsDeSaison", Title = "Nos miels", SortOrder = 3 },
                new HomeSection { Kind = "Calendrier", Title = "La ruche ce mois-ci", SortOrder = 4 });
        }

        await db.SaveChangesAsync();
    }
}
