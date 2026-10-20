(function () {
    var isAdmin = document.body.classList.contains('gb-admin');
    var SELECTORS = '.gb-admin-alert, .gb-shop-notice, .gb-alert, .gb-form-errors:not(.validation-summary-valid)';

    function decorate(root) {
        root.querySelectorAll(SELECTORS).forEach(function (el) {
            if (el.hasAttribute('data-gb-toast') || !el.textContent.trim()) return;
            el.setAttribute('data-gb-toast', '');
            var isError = el.classList.contains('alert-danger') || el.classList.contains('gb-form-errors');
            var message = el.innerHTML;
            el.remove();
            openModal(message, isError);
        });
    }

    // Modale native : le focus revient automatiquement sur le bouton qui a déclenché l'action.
    function openModal(html, isError) {
        if (!window.HTMLDialogElement) return;
        var opener = document.activeElement;
        var dialog = document.createElement('dialog');
        dialog.className = 'gb-notice-dialog ' + (isError ? 'gb-notice-dialog-error' : 'gb-notice-dialog-ok');
        dialog.setAttribute('role', isError ? 'alertdialog' : 'dialog');
        var body = document.createElement('div');
        body.className = 'gb-notice-dialog-body';
        body.innerHTML = html;
        var ok = document.createElement('button');
        ok.type = 'button';
        ok.className = 'gb-notice-dialog-ok-btn';
        ok.textContent = 'OK';
        ok.addEventListener('click', function () { dialog.close(); });
        dialog.appendChild(body);
        dialog.appendChild(ok);
        dialog.addEventListener('close', function () {
            dialog.remove();
            if (opener && document.contains(opener) && opener.focus) {
                opener.focus({ preventScroll: true });
            } else if (opener && opener.id) {
                var again = document.getElementById(opener.id);
                if (again) again.focus({ preventScroll: true });
            }
        });
        document.body.appendChild(dialog);
        try {
            dialog.showModal();
        } catch (err) {
            dialog.remove();
            return;
        }
        ok.focus();
    }

    function showError(message) {
        var el = document.createElement('div');
        el.className = isAdmin ? 'alert alert-danger gb-admin-alert' : 'gb-form-errors';
        el.setAttribute('role', 'alert');
        el.textContent = message;
        document.body.appendChild(el);
        decorate(document.body);
    }

    decorate(document);

    // Envoi des formulaires sans recharger la page : position et focus restent en place.
    document.addEventListener('submit', function (e) {
        var form = e.target;
        if (e.defaultPrevented || !(form instanceof HTMLFormElement)) return;
        if (form.method.toLowerCase() !== 'post') return;
        if (form.hasAttribute('data-gb-noajax') || form.querySelector('input[type=file]') || form.querySelector('textarea.gb-richtext')) return;
        if (!window.fetch || !window.DOMParser || !window.FormData) return;

        var submitter = e.submitter || null;
        var url = (submitter && submitter.hasAttribute('formaction') && submitter.formAction) || form.action;
        var data;
        try { data = new FormData(form, submitter); } catch (err) { return; }

        e.preventDefault();
        var y = window.scrollY;
        var activeId = document.activeElement && document.activeElement.id;
        var formIndex = Array.prototype.indexOf.call(document.querySelectorAll('main form'), form);
        var btnIndex = submitter ? Array.prototype.indexOf.call(form.querySelectorAll('button, input[type=submit]'), submitter) : -1;

        fetch(url, { method: 'POST', body: data, credentials: 'same-origin' })
            .then(function (r) { return r.text().then(function (html) { return { r: r, html: html }; }); })
            .then(function (res) {
                var finalUrl = new URL(res.r.url);
                if (!res.r.ok) { showError('Une erreur est survenue. Merci de r\u00e9essayer.'); return; }
                if (finalUrl.pathname !== location.pathname) { location.href = res.r.url; return; }

                var doc = new DOMParser().parseFromString(res.html, 'text/html');
                var newMain = doc.querySelector('main');
                var main = document.querySelector('main');
                if (!newMain || !main) { location.reload(); return; }

                main.innerHTML = newMain.innerHTML;
                var oldCart = document.querySelector('a[href="/Panier"]');
                var newCart = doc.querySelector('a[href="/Panier"]');
                if (oldCart && newCart) oldCart.textContent = newCart.textContent;

                var el = activeId ? document.getElementById(activeId) : null;
                if (!el && formIndex >= 0 && btnIndex >= 0) {
                    var newForm = main.querySelectorAll('form')[formIndex];
                    el = newForm && newForm.querySelectorAll('button, input[type=submit]')[btnIndex];
                }
                if (el) el.focus({ preventScroll: true });
                window.scrollTo(0, y);
                decorate(main);
                document.dispatchEvent(new CustomEvent('gb:content-replaced'));
            })
            .catch(function () { showError('Connexion impossible. Merci de r\u00e9essayer.'); });
    });
})();
