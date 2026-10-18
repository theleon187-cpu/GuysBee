---
name: paypal-checkout-razor
description: Paiement PayPal (Orders API v2) en Razor Pages avec livraison configurable.
---
- Client HTTP typé vers l'API PayPal (sandbox puis live), identifiants en User Secrets.
- Flux : création de la commande côté serveur (montant recalculé serveur), approbation par le client (bouton JS PayPal), capture côté serveur, vérification du montant et de la devise, passage en « payée ».
- Webhook `PAYMENT.CAPTURE.COMPLETED` avec vérification de signature.
- Livraison : `ShippingCalculator` pur et testable (mode, zone, poids, franco), alimenté par la configuration admin.
- Idempotence : une commande ne peut être capturée qu'une fois.
