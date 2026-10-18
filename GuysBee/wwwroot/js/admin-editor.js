// Initialisation de l'éditeur visuel pour toutes les zones de texte `.gb-richtext`.
// TinyMCE est servi localement (aucun appel à un service tiers).
document.addEventListener('DOMContentLoaded', function () {
    if (typeof tinymce === 'undefined') { return; }

    tinymce.init({
        selector: 'textarea.gb-richtext',
        license_key: 'gpl',
        height: 420,
        menubar: false,
        branding: false,
        promotion: false,
        plugins: 'lists link image table autoresize charmap wordcount',
        toolbar: 'undo redo | blocks | bold italic underline | bullist numlist | link image table | removeformat',
        block_formats: 'Paragraphe=p; Titre 2=h2; Titre 3=h3',
        link_default_target: '_blank',
        relative_urls: false,
        convert_urls: false,
        // Le HTML est de toute façon assaini côté serveur avant enregistrement.
        paste_data_images: false,
        content_style: 'body { font-family: sans-serif; font-size: 16px; line-height: 1.5; }'
    });
});
