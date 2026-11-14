(function () {
    var root = document.querySelector('[data-gb-motion]');
    if (!root || !('IntersectionObserver' in window) || !Element.prototype.animate) { return; }

    var reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)');
    if (reducedMotion.matches) { return; }

    var targets = root.querySelectorAll(
        '.gb-hero, .gb-hero-intro, .gb-section-title, .gb-prose, .gb-step, ' +
        '.gb-card-product, .gb-calendar-item, .gb-mosaic-item, ' +
        '.gb-home-admin .gb-admin-head, .gb-home-admin-list .gb-admin-card, .gb-home-admin-add'
    );
    var animations = new Set();
    var observer = new IntersectionObserver(function (entries) {
        entries.forEach(function (entry) {
            if (!entry.isIntersecting) { return; }
            observer.unobserve(entry.target);
            if (entry.target.contains(document.activeElement)) { return; }

            var isAdmin = !!entry.target.closest('.gb-home-admin');
            var frames = isAdmin
                ? [{ transform: 'translateY(12px)' }, { transform: 'translateY(0)' }]
                : [{ opacity: 0.6, transform: 'translateY(20px)' }, { opacity: 1, transform: 'translateY(0)' }];
            var animation = entry.target.animate(frames, {
                duration: isAdmin ? 400 : 650,
                easing: 'cubic-bezier(.22, 1, .36, 1)'
            });
            animations.add(animation);
            animation.onfinish = animation.oncancel = function () { animations.delete(animation); };
        });
    }, { threshold: 0.05 });

    targets.forEach(function (target) { observer.observe(target); });
    reducedMotion.addEventListener('change', function (event) {
        if (!event.matches) { return; }
        observer.disconnect();
        animations.forEach(function (animation) { animation.cancel(); });
        animations.clear();
    });
}());
