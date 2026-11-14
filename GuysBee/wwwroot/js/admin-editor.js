// Initialisation de l'éditeur visuel pour toutes les zones de texte `.gb-richtext`.
// TinyMCE est servi localement (aucun appel à un service tiers).
document.addEventListener('DOMContentLoaded', function () {
    if (typeof tinymce === 'undefined') { return; }

    var canUpload = !!document.querySelector('textarea.gb-richtext[data-upload-url]');

    // Reprend les polices choisies pour le site : ce qui est vu dans l'éditeur est ce qui s'affichera.
    var fonts = window.gbEditorFonts || {};
    function fontFor(el) { return (el.dataset.font === 'brand' ? fonts.brand : null) || fonts.text || 'sans-serif'; }
    var IMG_CSS = 'img { max-width: 100%; height: auto; display: block; margin: 1rem 0; border-radius: 8px; }';
    function setupEditor(editor) {
        editor.on('init', function () {
            var el = editor.getElement();
            var css = (fonts.css || '') + IMG_CSS + (fonts.title ? ' h1, h2, h3 { font-family: ' + fonts.title + '; }' : '');
            editor.dom.addStyle(css);
            editor.getBody().style.fontFamily = fontFor(el);
            if (el.dataset.font === 'brand') {
                var select = document.getElementById('BrandFont');
                if (select) {
                    select.addEventListener('change', function () {
                        editor.getBody().style.fontFamily = (fonts.stacks && fonts.stacks[select.value]) || fonts.brand || fonts.text || 'sans-serif';
                    });
                    select.dispatchEvent(new Event('change'));
                }
            }
        });
    }

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
        toolbar: 'undo redo | blocks fontsize | bold italic underline strikethrough | forecolor backcolor | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | link ' + (canUpload ? 'image ' : '') + 'table charmap | removeformat',
        font_size_formats: '12px 14px 16px 18px 20px 24px 28px 32px 40px 48px',
        block_formats: 'Paragraphe=p; Titre 2=h2; Titre 3=h3; Citation=blockquote',
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
        setup: setupEditor,
        content_style: 'body { font-size: 16px; line-height: 1.5; }'
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
        toolbar: 'fontsize | bold italic underline | forecolor backcolor | alignleft aligncenter alignright | bullist numlist | link | removeformat',
        font_size_formats: '12px 14px 16px 18px 20px 24px 28px 32px',
        link_default_target: '_blank',
        relative_urls: false,
        convert_urls: false,
        setup: setupEditor,
        content_style: 'body { font-size: 15px; line-height: 1.5; }'
    });
});
