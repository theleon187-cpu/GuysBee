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
    var dialog = document.createElement('dialog');
    dialog.className = 'gb-lightbox';
    dialog.innerHTML = '<button type=\"button\" class=\"gb-lightbox-close\" aria-label=\"Fermer\">&times;</button><img class=\"gb-lightbox-img\" alt=\"\" />';
    document.body.appendChild(dialog);
    var big = dialog.querySelector('img');
    gallery.addEventListener('click', function (e) {
        var img = e.target.closest('img');
        if (!img) { return; }
        big.src = img.dataset.full || img.src;
        big.alt = img.alt;
        dialog.showModal();
    });
    dialog.addEventListener('click', function () { dialog.close(); });
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
