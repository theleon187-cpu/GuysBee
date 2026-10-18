document.addEventListener('DOMContentLoaded', function () {
    var toggle = document.querySelector('.gb-nav-toggle');
    var nav = document.getElementById('gb-nav');
    if (!toggle || !nav) { return; }

    toggle.addEventListener('click', function () {
        var open = nav.classList.toggle('gb-nav-open');
        toggle.setAttribute('aria-expanded', open ? 'true' : 'false');
    });
});
