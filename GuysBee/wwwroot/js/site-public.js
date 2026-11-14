document.addEventListener('DOMContentLoaded', function () {
    var toggle = document.querySelector('.gb-nav-toggle');
    var nav = document.getElementById('gb-nav');
    if (!toggle || !nav) { return; }

    toggle.addEventListener('click', function () {
        var open = nav.classList.toggle('gb-nav-open');
        toggle.setAttribute('aria-expanded', open ? 'true' : 'false');
    });
});

document.addEventListener('DOMContentLoaded', function () {
    var gallery = document.querySelector('[data-gb-lightbox]');
    if (!gallery || !window.HTMLDialogElement) { return; }
    var images = Array.prototype.slice.call(gallery.querySelectorAll('img'));
    if (!images.length) { return; }

    var dialog = document.createElement('dialog');
    dialog.className = 'gb-lightbox';
    dialog.setAttribute('aria-label', 'Visionneuse de photos');
    dialog.innerHTML =
        '<button type="button" class="gb-lightbox-close" aria-label="Fermer">&times;</button>' +
        '<button type="button" class="gb-lightbox-nav gb-lightbox-prev" aria-label="Photo précédente">&#8249;</button>' +
        '<figure class="gb-lightbox-figure"><img class="gb-lightbox-img" alt="" />' +
        '<figcaption class="gb-lightbox-caption"></figcaption></figure>' +
        '<button type="button" class="gb-lightbox-nav gb-lightbox-next" aria-label="Photo suivante">&#8250;</button>' +
        '<p class="gb-lightbox-count" aria-live="polite"></p>';
    document.body.appendChild(dialog);

    var big = dialog.querySelector('.gb-lightbox-img');
    var caption = dialog.querySelector('.gb-lightbox-caption');
    var count = dialog.querySelector('.gb-lightbox-count');
    var index = 0;

    function show(i) {
        index = (i + images.length) % images.length;
        var img = images[index];
        big.src = img.dataset.full || img.src;
        big.alt = img.alt;
        caption.textContent = img.dataset.caption || '';
        count.textContent = (index + 1) + ' / ' + images.length;
        var next = images[(index + 1) % images.length];
        if (next) { new Image().src = next.dataset.full || next.src; }
    }

    gallery.addEventListener('click', function (e) {
        var img = e.target.closest('img');
        if (!img) { return; }
        show(images.indexOf(img));
        dialog.showModal();
    });
    dialog.querySelector('.gb-lightbox-prev').addEventListener('click', function (e) { e.stopPropagation(); show(index - 1); });
    dialog.querySelector('.gb-lightbox-next').addEventListener('click', function (e) { e.stopPropagation(); show(index + 1); });
    dialog.querySelector('.gb-lightbox-close').addEventListener('click', function () { dialog.close(); });
    dialog.addEventListener('click', function (e) { if (e.target === dialog) { dialog.close(); } });
    dialog.addEventListener('keydown', function (e) {
        if (e.key === 'ArrowLeft') { show(index - 1); }
        else if (e.key === 'ArrowRight') { show(index + 1); }
    });

    var startX = null;
    dialog.addEventListener('touchstart', function (e) { startX = e.touches[0].clientX; }, { passive: true });
    dialog.addEventListener('touchend', function (e) {
        if (startX === null) { return; }
        var dx = e.changedTouches[0].clientX - startX;
        if (Math.abs(dx) > 40) { show(index + (dx < 0 ? 1 : -1)); }
        startX = null;
    });
});

document.addEventListener('DOMContentLoaded', function () {
    var slides = document.querySelectorAll('.gb-hero-media');
    if (slides.length < 2 || window.matchMedia('(prefers-reduced-motion: reduce)').matches) { return; }
    var current = 0;
    setInterval(function () {
        slides[current].classList.remove('gb-hero-active');
        current = (current + 1) % slides.length;
        slides[current].classList.add('gb-hero-active');
    }, 6000);
});
