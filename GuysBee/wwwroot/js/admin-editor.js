// Initialisation de l'éditeur visuel pour toutes les zones de texte `.gb-richtext`.
// TinyMCE est servi localement (aucun appel à un service tiers).
document.addEventListener('DOMContentLoaded', function () {
    if (typeof tinymce === 'undefined') { return; }

    var canUpload = !!document.querySelector('textarea.gb-richtext[data-upload-url]');

    tinymce.init({
        selector: 'textarea.gb-richtext',
        license_key: 'gpl',
        height: 420,
        min_height: 200,
        resize: true,
        menubar: false,
        branding: false,
        promotion: false,
        plugins: 'lists link image table charmap wordcount',
        toolbar: 'undo redo | blocks | bold italic underline | bullist numlist | link ' + (canUpload ? 'image ' : '') + 'table | removeformat',
        block_formats: 'Paragraphe=p; Titre 2=h2; Titre 3=h3',
        link_default_target: '_blank',
        relative_urls: false,
        convert_urls: false,
        // Le HTML est de toute façon assaini côté serveur avant enregistrement.
        paste_data_images: false,
        image_title: true,
        image_dimensions: false,
        images_file_types: 'jpg,jpeg,png,webp',
        automatic_uploads: true,
        images_upload_handler: function (blobInfo) {
            var area = document.querySelector('textarea.gb-richtext[data-upload-url]');
            var tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
            if (!area) { return Promise.reject({ message: 'Envoi de photo indisponible ici.', remove: true }); }
            var data = new FormData();
            data.append('file', blobInfo.blob(), blobInfo.filename());
            if (tokenInput) { data.append('__RequestVerificationToken', tokenInput.value); }
            return fetch(area.dataset.uploadUrl, { method: 'POST', body: data, credentials: 'same-origin' })
                .then(function (r) { return r.json().then(function (j) { return { ok: r.ok, j: j }; }); })
                .then(function (res) {
                    if (!res.ok || !res.j.location) { throw { message: res.j.error || 'Envoi impossible.', remove: true }; }
                    return res.j.location;
                });
        },
        content_style: 'body { font-family: sans-serif; font-size: 16px; line-height: 1.5; }'
    });

    tinymce.init({
        selector: 'textarea.gb-richtext-compact',
        license_key: 'gpl',
        height: 220,
        min_height: 120,
        resize: true,
        menubar: false,
        branding: false,
        promotion: false,
        plugins: 'lists link',
        toolbar: 'bold italic underline | bullist numlist | link | removeformat',
        link_default_target: '_blank',
        relative_urls: false,
        convert_urls: false,
        content_style: 'body { font-family: sans-serif; font-size: 15px; line-height: 1.5; }'
    });
});
