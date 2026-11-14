(function () {
    var root = document.querySelector('[data-gb-admin-motion]');
    if (!root || !('IntersectionObserver' in window) || !Element.prototype.animate) { return; }

    var reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)');
    if (reducedMotion.matches) { return; }

    var selectors = [
        '.gb-admin-head',
        '.gb-admin-card',
        '.gb-panel',
        '.gb-stat',
        '.gb-quick-item',
        '.table',
        '.alert'
    ].join(',');

    var targets = root.querySelectorAll(selectors);
    if (!targets.length) { return; }

    var observer = new IntersectionObserver(function (entries) {
        entries.forEach(function (entry) {
            if (!entry.isIntersecting) { return; }
            observer.unobserve(entry.target);

            if (entry.target.contains(document.activeElement)) { return; }

            var duration = entry.target.matches('.gb-quick-item, .gb-stat') ? 360 : 520;
            var distance = entry.target.matches('.gb-quick-item, .gb-stat') ? 10 : 18;

            entry.target.animate([
                { opacity: .01, transform: 'translate3d(0,' + distance + 'px,0)' },
                { opacity: 1, transform: 'translate3d(0,0,0)' }
            ], {
                duration: duration,
                easing: 'cubic-bezier(.22, 1, .36, 1)',
                fill: 'both'
            });
        });
    }, {
        threshold: 0.1,
        rootMargin: '0px 0px -8% 0px'
    });

    targets.forEach(function (target) {
        observer.observe(target);
    });

    reducedMotion.addEventListener('change', function (event) {
        if (!event.matches) { return; }
        observer.disconnect();
    });
}());
