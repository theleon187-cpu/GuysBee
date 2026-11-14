document.addEventListener('DOMContentLoaded', function () {
    var toggle = document.querySelector('.gb-nav-toggle');
    var nav = document.getElementById('gb-nav');
    if (!toggle || !nav) { return; }

    nav.querySelectorAll('.gb-menu-item-parent').forEach(function (item) {
        var link = item.querySelector('.gb-menu-link');
        var btn = document.createElement('button');
        btn.type = 'button';
        btn.className = 'gb-sub-toggle';
        item.classList.add('gb-sub-open');
        btn.setAttribute('aria-expanded', 'true');
        btn.setAttribute('aria-label', 'Afficher ou masquer le sous-menu ' + (link ? link.textContent.trim() : ''));
        btn.addEventListener('click', function () {
            var open = item.classList.toggle('gb-sub-open');
            btn.setAttribute('aria-expanded', open ? 'true' : 'false');
        });
        link.insertAdjacentElement('afterend', btn);
    });

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
    var heroes = document.querySelectorAll('[data-gb-hero]');
    if (!heroes.length) { return; }

    heroes.forEach(function (hero) {
        var slides = Array.prototype.slice.call(hero.querySelectorAll('.gb-hero-media'));
        if (slides.length < 2) { return; }

        var autoplayDelay = 3800;
        var dots = Array.prototype.slice.call(hero.querySelectorAll('[data-gb-hero-dot]'));
        var prev = hero.querySelector('[data-gb-hero-prev]');
        var next = hero.querySelector('[data-gb-hero-next]');
        var toggle = hero.querySelector('[data-gb-hero-toggle]');
        var progressBar = hero.querySelector('[data-gb-hero-progress]');
        var currentLabel = hero.querySelector('[data-gb-hero-current]');
        var reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)');
        var current = Math.max(0, slides.findIndex(function (s) { return s.classList.contains('gb-hero-active'); }));
        var timer = null;
        var timerStartedAt = 0;
        var remainingDelay = autoplayDelay;
        var startX = null;
        var startY = null;
        var isPaused = false;

        function resetProgress() {
            if (!progressBar) { return; }
            progressBar.style.transition = 'none';
            progressBar.style.transform = 'scaleX(0)';
        }

        function animateProgress(delay, elapsed) {
            if (!progressBar || reducedMotion.matches || isPaused) { return; }
            var ratio = Math.max(0, Math.min(1, elapsed / autoplayDelay));
            progressBar.style.transition = 'none';
            progressBar.style.transform = 'scaleX(' + ratio + ')';
            requestAnimationFrame(function () {
                requestAnimationFrame(function () {
                    progressBar.style.transition = 'transform ' + delay + 'ms linear';
                    progressBar.style.transform = 'scaleX(1)';
                });
            });
        }

        function setActive(index) {
            current = (index + slides.length) % slides.length;
            slides.forEach(function (slide, i) {
                slide.classList.toggle('gb-hero-active', i === current);
            });
            dots.forEach(function (dot, i) {
                var isActive = i === current;
                dot.classList.toggle('gb-hero-dot-active', isActive);
                dot.setAttribute('aria-selected', isActive ? 'true' : 'false');
                dot.setAttribute('tabindex', isActive ? '0' : '-1');
            });
            if (currentLabel) {
                currentLabel.textContent = (current + 1).toString();
            }
        }

        function stopAutoplay(keepProgress) {
            if (timer) {
                clearTimeout(timer);
                timer = null;
                if (keepProgress) {
                    var elapsed = Date.now() - timerStartedAt;
                    remainingDelay = Math.max(120, remainingDelay - elapsed);
                }
            }

            if (keepProgress) {
                if (progressBar) {
                    var keptElapsed = autoplayDelay - remainingDelay;
                    progressBar.style.transition = 'none';
                    progressBar.style.transform = 'scaleX(' + Math.max(0, Math.min(1, keptElapsed / autoplayDelay)) + ')';
                }
                return;
            }

            remainingDelay = autoplayDelay;
            resetProgress();
        }

        function scheduleAutoplay(keepProgress) {
            if (timer) {
                clearTimeout(timer);
                timer = null;
            }
            if (!keepProgress) {
                remainingDelay = autoplayDelay;
                resetProgress();
            }
            if (reducedMotion.matches || isPaused) { return; }

            var delay = keepProgress ? Math.max(120, remainingDelay) : autoplayDelay;
            var elapsed = autoplayDelay - delay;
            animateProgress(delay, elapsed);
            timerStartedAt = Date.now();
            remainingDelay = delay;
            timer = setTimeout(function () {
                remainingDelay = autoplayDelay;
                setActive(current + 1);
                scheduleAutoplay(false);
            }, delay);
        }

        function setPaused(paused) {
            isPaused = paused;
            hero.classList.toggle('gb-hero-paused', paused);
            if (toggle) {
                toggle.textContent = paused ? 'Reprendre' : 'Pause';
                toggle.setAttribute('aria-pressed', paused ? 'true' : 'false');
            }
            if (paused) {
                stopAutoplay(true);
            } else {
                scheduleAutoplay(true);
            }
        }

        function moveTo(index) {
            setActive(index);
            if (!isPaused) {
                scheduleAutoplay(false);
            }
        }

        if (prev) {
            prev.addEventListener('click', function () {
                moveTo(current - 1);
            });
        }

        if (next) {
            next.addEventListener('click', function () {
                moveTo(current + 1);
            });
        }

        if (toggle) {
            toggle.addEventListener('click', function () {
                setPaused(!isPaused);
            });
        }

        dots.forEach(function (dot) {
            dot.addEventListener('click', function () {
                var index = parseInt(dot.getAttribute('data-gb-index'), 10);
                if (isNaN(index)) { return; }
                moveTo(index);
            });
        });

        hero.addEventListener('keydown', function (event) {
            if (event.key === 'ArrowLeft') {
                event.preventDefault();
                moveTo(current - 1);
            } else if (event.key === 'ArrowRight') {
                event.preventDefault();
                moveTo(current + 1);
            } else if (event.key === ' ' || event.key === 'Spacebar') {
                event.preventDefault();
                setPaused(!isPaused);
            }
        });

        hero.addEventListener('pointerdown', function (event) {
            if (event.pointerType === 'mouse') { return; }
            startX = event.clientX;
            startY = event.clientY;
        });

        hero.addEventListener('pointerup', function (event) {
            if (startX === null || startY === null) { return; }
            var dx = event.clientX - startX;
            var dy = event.clientY - startY;
            startX = null;
            startY = null;
            if (Math.abs(dx) < 45 || Math.abs(dx) < Math.abs(dy)) { return; }
            moveTo(current + (dx < 0 ? 1 : -1));
        });

        hero.addEventListener('focusin', function () {
            if (!isPaused) { stopAutoplay(true); }
        });
        hero.addEventListener('focusout', function (event) {
            if (hero.contains(event.relatedTarget) || isPaused) { return; }
            scheduleAutoplay(true);
        });

        reducedMotion.addEventListener('change', function () {
            if (!reducedMotion.matches && !isPaused) {
                scheduleAutoplay(true);
                return;
            }
            stopAutoplay(true);
        });

        setActive(current);
        setPaused(false);
    });
});

document.addEventListener('click', function (e) {
    var btn = e.target.closest('[data-gb-qty-step]');
    if (!btn) { return; }
    var input = btn.closest('.gb-qty').querySelector('input[name="quantity"]');
    var min = parseInt(input.min, 10) || 1;
    var max = parseInt(input.max, 10) || 99;
    var value = (parseInt(input.value, 10) || min) + parseInt(btn.getAttribute('data-gb-qty-step'), 10);
    input.value = Math.max(min, Math.min(max, value));
});

document.addEventListener('submit', function (e) {
    var form = e.target;
    if (!form.matches || !form.matches('form.gb-shop-inline') || !window.fetch) { return; }
    e.preventDefault();
    var button = form.querySelector('button[type="submit"]');
    if (button) { button.disabled = true; }

    fetch(form.action, {
        method: 'POST',
        body: new FormData(form),
        headers: { 'X-Requested-With': 'fetch' },
        credentials: 'same-origin'
    }).then(function (r) {
        if (!r.ok) { throw new Error(r.status); }
        return r.json();
    }).then(function (data) {
        var link = document.querySelector('.gb-cart-link');
        if (link) {
            var badge = link.querySelector('.gb-cart-badge');
            if (!badge) {
                badge = document.createElement('span');
                badge.className = 'gb-cart-badge';
                link.appendChild(badge);
            }
            badge.textContent = data.count;
            link.setAttribute('aria-label', 'Panier, ' + data.count + ' article(s)');
        }
        showCartToast(data.message);
    }).catch(function () {
        form.submit();
    }).then(function () {
        if (button) { button.disabled = false; }
    });
});

function showCartToast(message) {
    var toast = document.getElementById('gb-cart-toast');
    if (!toast) {
        toast = document.createElement('div');
        toast.id = 'gb-cart-toast';
        toast.className = 'gb-toast';
        toast.setAttribute('role', 'status');
        toast.setAttribute('aria-live', 'polite');
        document.body.appendChild(toast);
    }
    toast.textContent = message;
    toast.classList.add('gb-toast-show');
    clearTimeout(toast._timer);
    toast._timer = setTimeout(function () { toast.classList.remove('gb-toast-show'); }, 2500);
}
(function(){var t=new WeakMap();function post(f){return fetch(f.action,{method:'POST',body:new FormData(f),headers:{'X-Requested-With':'fetch'},credentials:'same-origin'}).then(function(r){if(!r.ok){throw new Error(r.status);}return r.json();});}function badge(c){var l=document.querySelector('.gb-cart-link');if(!l){return;}var b=l.querySelector('.gb-cart-badge');if(!b&&c>0){b=document.createElement('span');b.className='gb-cart-badge';l.appendChild(b);}if(b){if(c>0){b.textContent=c;}else{b.remove();}}l.setAttribute('aria-label','Panier, '+c+' article(s)');}function apply(i,d){var T=document.querySelector('[data-cart-total]');if(T){T.textContent=d.total;}badge(d.count);if(d.empty){location.reload();return;}if(d.quantity===0){i.remove();return;}var L=i.querySelector('[data-cart-line]');if(L){L.textContent=d.lineTotal;}i.classList.remove('gb-cart-busy');}function send(i,f){i.classList.add('gb-cart-busy');post(f).then(function(d){apply(i,d);}).catch(function(){f.submit();});}document.addEventListener('click',function(e){var b=e.target.closest('[data-cart-step]');if(!b){return;}var f=b.closest('form'),i=b.closest('[data-cart-item]'),n=f.querySelector('input[name=quantity]');var v=(parseInt(n.value,10)||1)+parseInt(b.getAttribute('data-cart-step'),10);if(v<1){return;}n.value=Math.min(99,v);clearTimeout(t.get(f));t.set(f,setTimeout(function(){send(i,f);},350));});document.addEventListener('change',function(e){var n=e.target;if(!n.matches||!n.matches('.gb-cart-form input[name=quantity]')){return;}var v=parseInt(n.value,10);n.value=isNaN(v)||v<1?1:Math.min(99,v);send(n.closest('[data-cart-item]'),n.closest('form'));});document.addEventListener('submit',function(e){var f=e.target;if(!f.matches||!f.matches('form.gb-cart-form')||!window.fetch){return;}e.preventDefault();send(f.closest('[data-cart-item]'),f);});})();
