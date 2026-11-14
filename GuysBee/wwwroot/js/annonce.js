(function () {
    var el = document.querySelector('[data-gb-annonce]');
    if (!el) return;
    var name = 'gb_annonce';
    var version = el.getAttribute('data-version') || '1';
    var days = parseInt(el.getAttribute('data-days'), 10) || 0;
    var delay = (parseInt(el.getAttribute('data-delay'), 10) || 0) * 1000;
    var isModal = el.classList.contains('gb-annonce-fenetre');

    function readCookie() {
        var m = document.cookie.match(new RegExp('(?:^|; )' + name + '=([^;]*)'));
        return m ? decodeURIComponent(m[1]) : null;
    }
    function writeCookie() {
        var c = name + '=' + encodeURIComponent(version) + '; path=/; SameSite=Lax';
        if (location.protocol === 'https:') c += '; Secure';
        if (days > 0) c += '; max-age=' + (days * 86400);
        document.cookie = c;
    }

    if (readCookie() === version) return;

    var previous = null;
    function close() {
        el.hidden = true;
        document.body.classList.remove('gb-annonce-ouverte');
        writeCookie();
        if (previous && previous.focus) previous.focus();
    }
    function show() {
        el.hidden = false;
        if (isModal) {
            document.body.classList.add('gb-annonce-ouverte');
            previous = document.activeElement;
            var btn = el.querySelector('[data-gb-annonce-close]');
            if (btn) btn.focus();
        }
    }

    var closeBtn = el.querySelector('[data-gb-annonce-close]');
    if (closeBtn) closeBtn.addEventListener('click', close);
    if (isModal && closeBtn) {
        el.addEventListener('click', function (e) { if (e.target === el) close(); });
        document.addEventListener('keydown', function (e) { if (e.key === 'Escape' && !el.hidden) close(); });
    }

    if (delay > 0) setTimeout(show, delay); else show();
})();
