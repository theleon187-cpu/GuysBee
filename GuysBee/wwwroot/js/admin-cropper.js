// Rognage et redimensionnement des images avant envoi (tous les champs "fichier" image de l'administration).
(function () {
    var IMAGE_TYPES = ['image/jpeg', 'image/png', 'image/webp'];
    var MIN = 24;
    var dlg, stage, img, box, ratioSel, widthSel, info, btnOk, btnKeep, btnCancel, counter, previewCanvas;
    var current = null;

    function isImageInput(input) {
        if (input.type !== 'file') { return false; }
        var accept = (input.getAttribute('accept') || '').toLowerCase();
        return accept.indexOf('image/') >= 0 || /\.(jpe?g|png|webp)/.test(accept);
    }

    function build() {
        if (dlg) { return; }
        dlg = document.createElement('div');
        dlg.className = 'gb-crop-overlay';
        dlg.hidden = true;
        dlg.innerHTML =
            '<div class="gb-crop-dialog" role="dialog" aria-modal="true" aria-label="Rogner et redimensionner la photo">' +
            '<h2 class="h5 gb-crop-title">Rogner et redimensionner la photo <span class="gb-crop-counter"></span></h2>' +
            '<p class="gb-admin-help">Déplacez le cadre ou tirez sur ses coins pour choisir la partie à garder.</p>' +
            '<div class="gb-crop-layout"><div class="gb-crop-stage"><img alt="Aperçu de la photo à rogner" /><div class="gb-crop-box">' +
            '<span class="gb-crop-handle gb-crop-nw" data-h="nw"></span><span class="gb-crop-handle gb-crop-ne" data-h="ne"></span>' +
            '<span class="gb-crop-handle gb-crop-sw" data-h="sw"></span><span class="gb-crop-handle gb-crop-se" data-h="se"></span>' +
            '</div></div><aside class="gb-crop-live" aria-live="polite"><p class="gb-crop-live-title">Aperçu en direct</p><canvas class="gb-crop-preview" width="260" height="260"></canvas><p class="gb-crop-live-help">Mise à jour immédiate selon votre cadrage.</p></aside></div>' +
            '<div class="gb-crop-options">' +
            '<label>Forme du cadre <select class="form-select form-select-sm gb-crop-ratio">' +
            '<option value="0">Libre</option><option value="1">Carré</option><option value="1.3333">Paysage 4:3</option>' +
            '<option value="1.5">Paysage 3:2</option><option value="1.7778">Large 16:9</option><option value="0.75">Portrait 3:4</option></select></label>' +
            '<label>Taille finale <select class="form-select form-select-sm gb-crop-width">' +
            '<option value="0">Taille d\'origine</option><option value="1600">1600 px de large</option><option value="1200">1200 px de large</option>' +
            '<option value="800">800 px de large</option><option value="480">480 px de large</option><option value="256">256 px de large</option></select></label>' +
            '</div><p class="gb-admin-help gb-crop-info"></p>' +
            '<div class="gb-crop-actions">' +
            '<button type="button" class="btn btn-warning gb-crop-ok">Valider</button>' +
            '<button type="button" class="btn btn-outline-dark gb-crop-keep">Garder la photo telle quelle</button>' +
            '<button type="button" class="btn btn-link gb-crop-cancel">Annuler</button></div></div>';
        document.body.appendChild(dlg);
        stage = dlg.querySelector('.gb-crop-stage');
        img = stage.querySelector('img');
        box = stage.querySelector('.gb-crop-box');
        ratioSel = dlg.querySelector('.gb-crop-ratio');
        widthSel = dlg.querySelector('.gb-crop-width');
        info = dlg.querySelector('.gb-crop-info');
        counter = dlg.querySelector('.gb-crop-counter');
        previewCanvas = dlg.querySelector('.gb-crop-preview');
        btnOk = dlg.querySelector('.gb-crop-ok');
        btnKeep = dlg.querySelector('.gb-crop-keep');
        btnCancel = dlg.querySelector('.gb-crop-cancel');

        ratioSel.addEventListener('change', function () { applyRatio(); });
        widthSel.addEventListener('change', updateInfo);
        box.addEventListener('pointerdown', startDrag);
        document.addEventListener('keydown', function (e) { if (!dlg.hidden && e.key === 'Escape') { btnCancel.click(); } });
    }

    function ratio() { return parseFloat(ratioSel.value) || 0; }
    function dispW() { return img.clientWidth; }
    function dispH() { return img.clientHeight; }

    function paint() {
        var r = current.rect;
        box.style.left = r.x + 'px';
        box.style.top = r.y + 'px';
        box.style.width = r.w + 'px';
        box.style.height = r.h + 'px';
        updateInfo();
    }

    function outputSize() {
        var k = img.naturalWidth / dispW();
        var w = Math.round(current.rect.w * k), h = Math.round(current.rect.h * k);
        var max = parseInt(widthSel.value, 10) || 0;
        if (max > 0 && w > max) { h = Math.round(h * max / w); w = max; }
        return { w: Math.max(1, w), h: Math.max(1, h) };
    }

    function updateInfo() {
        var o = outputSize();
        info.textContent = 'Taille de la photo obtenue : ' + o.w + ' × ' + o.h + ' px.';
        updatePreview(o);
    }

    function updatePreview(o) {
        if (!previewCanvas || !current || !current.rect || !img.naturalWidth || !img.naturalHeight) { return; }
        var rect = current.rect;
        var k = img.naturalWidth / dispW();
        var srcX = rect.x * k;
        var srcY = rect.y * k;
        var srcW = rect.w * k;
        var srcH = rect.h * k;

        var maxEdge = 260;
        var scale = Math.min(1, maxEdge / Math.max(o.w, o.h));
        var pw = Math.max(1, Math.round(o.w * scale));
        var ph = Math.max(1, Math.round(o.h * scale));

        previewCanvas.width = pw;
        previewCanvas.height = ph;
        var ctx = previewCanvas.getContext('2d');
        if (!ctx) { return; }
        ctx.clearRect(0, 0, pw, ph);
        ctx.imageSmoothingQuality = 'high';
        ctx.drawImage(img, srcX, srcY, srcW, srcH, 0, 0, pw, ph);
    }

    function applyRatio() {
        var W = dispW(), H = dispH(), r = ratio(), rect = current.rect;
        if (r) {
            var w = rect.w, h = w / r;
            if (h > H) { h = H; w = h * r; }
            if (w > W) { w = W; h = w / r; }
            var cx = rect.x + rect.w / 2, cy = rect.y + rect.h / 2;
            rect.w = w; rect.h = h;
            rect.x = Math.min(Math.max(0, cx - w / 2), W - w);
            rect.y = Math.min(Math.max(0, cy - h / 2), H - h);
        }
        paint();
    }

    function startDrag(e) {
        e.preventDefault();
        var handle = e.target.getAttribute('data-h');
        var start = { x: e.clientX, y: e.clientY, rect: { x: current.rect.x, y: current.rect.y, w: current.rect.w, h: current.rect.h } };
        box.setPointerCapture(e.pointerId);
        function move(ev) {
            var dx = ev.clientX - start.x, dy = ev.clientY - start.y, W = dispW(), H = dispH(), s = start.rect;
            if (!handle) {
                current.rect.x = Math.min(Math.max(0, s.x + dx), W - s.w);
                current.rect.y = Math.min(Math.max(0, s.y + dy), H - s.h);
            } else {
                var l = s.x, t = s.y, rr = s.x + s.w, b = s.y + s.h;
                if (handle.indexOf('w') >= 0) { l = Math.min(Math.max(0, s.x + dx), rr - MIN); }
                if (handle.indexOf('e') >= 0) { rr = Math.max(Math.min(W, rr + dx), l + MIN); }
                if (handle.indexOf('n') >= 0) { t = Math.min(Math.max(0, s.y + dy), b - MIN); }
                if (handle.indexOf('s') >= 0) { b = Math.max(Math.min(H, b + dy), t + MIN); }
                var r = ratio();
                if (r) {
                    var w = rr - l, h = w / r;
                    var maxH = handle.indexOf('n') >= 0 ? b : H - t;
                    if (h > maxH) { h = maxH; w = h * r; }
                    if (handle.indexOf('w') >= 0) { l = rr - w; } else { rr = l + w; }
                    if (handle.indexOf('n') >= 0) { t = b - h; } else { b = t + h; }
                }
                current.rect = { x: l, y: t, w: rr - l, h: b - t };
            }
            paint();
        }
        function up() {
            box.removeEventListener('pointermove', move);
            box.removeEventListener('pointerup', up);
            box.removeEventListener('pointercancel', up);
        }
        box.addEventListener('pointermove', move);
        box.addEventListener('pointerup', up);
        box.addEventListener('pointercancel', up);
    }

    function ensureFinalPreview(input) {
        var wrap = input.parentElement && input.parentElement.querySelector('.gb-crop-final-wrap');
        if (wrap) { return wrap; }
        wrap = document.createElement('div');
        wrap.className = 'gb-crop-final-wrap';
        input.insertAdjacentElement('afterend', wrap);
        return wrap;
    }

    function clearFinalPreview(input) {
        var wrap = input.parentElement && input.parentElement.querySelector('.gb-crop-final-wrap');
        if (!wrap) { return; }
        wrap.innerHTML = '';
        wrap.hidden = true;
    }

    function renderFinalPreview(input, files) {
        if (!files || files.length === 0) { clearFinalPreview(input); return; }
        var wrap = ensureFinalPreview(input);
        wrap.hidden = false;
        wrap.innerHTML = '<p class="gb-crop-final-title">Rendu validé</p>';

        var list = document.createElement('div');
        list.className = 'gb-crop-final-list';
        wrap.appendChild(list);

        files.forEach(function (file) {
            if (IMAGE_TYPES.indexOf(file.type) < 0) { return; }
            var item = document.createElement('figure');
            item.className = 'gb-crop-final-item';

            var image = document.createElement('img');
            image.className = 'gb-crop-final-img';
            image.alt = 'Aperçu validé de ' + file.name;
            var url = URL.createObjectURL(file);
            image.src = url;
            image.onload = function () {
                var cap = item.querySelector('figcaption');
                if (cap) { cap.textContent = file.name + ' · ' + image.naturalWidth + ' × ' + image.naturalHeight + ' px'; }
                URL.revokeObjectURL(url);
            };

            var caption = document.createElement('figcaption');
            caption.textContent = file.name;
            item.appendChild(image);
            item.appendChild(caption);
            list.appendChild(item);
        });
    }

    function openFor(file, index, total) {
        build();
        return new Promise(function (resolve) {
            var url = URL.createObjectURL(file);
            current = { rect: null };
            counter.textContent = total > 1 ? '(' + index + ' sur ' + total + ')' : '';
            ratioSel.value = '0';
            widthSel.value = '0';

            function finish(result) {
                dlg.hidden = true;
                document.body.classList.remove('gb-crop-open');
                btnOk.onclick = btnKeep.onclick = btnCancel.onclick = null;
                img.removeAttribute('src');
                URL.revokeObjectURL(url);
                resolve(result);
            }

            img.onload = function () {
                var W = dispW(), H = dispH();
                current.rect = { x: 0, y: 0, w: W, h: H };
                var hint = (document.activeElement && document.activeElement.dataset && document.activeElement.dataset.cropRatio) || '';
                if (hint) { ratioSel.value = hint; }
                applyRatio();
                btnOk.focus();
            };
            img.onerror = function () { finish('keep'); };

            btnKeep.onclick = function () { finish('keep'); };
            btnCancel.onclick = function () { finish('cancel'); };
            btnOk.onclick = function () {
                var k = img.naturalWidth / dispW(), r = current.rect, o = outputSize();
                var canvas = document.createElement('canvas');
                canvas.width = o.w; canvas.height = o.h;
                var ctx = canvas.getContext('2d');
                ctx.imageSmoothingQuality = 'high';
                ctx.drawImage(img, r.x * k, r.y * k, r.w * k, r.h * k, 0, 0, o.w, o.h);
                var type = IMAGE_TYPES.indexOf(file.type) >= 0 ? file.type : 'image/jpeg';
                canvas.toBlob(function (blob) {
                    if (!blob) { finish('keep'); return; }
                    var ext = type === 'image/png' ? '.png' : type === 'image/webp' ? '.webp' : '.jpg';
                    var name = file.name.replace(/\.[^.]+$/, '') + ext;
                    finish(new File([blob], name, { type: type, lastModified: Date.now() }));
                }, type, 0.92);
            };

            dlg.hidden = false;
            document.body.classList.add('gb-crop-open');
            img.src = url;
        });
    }

    async function process(input) {
        var files = Array.prototype.slice.call(input.files);
        var out = [];
        for (var i = 0; i < files.length; i++) {
            var f = files[i];
            if (IMAGE_TYPES.indexOf(f.type) < 0) { out.push(f); continue; }
            var res = await openFor(f, i + 1, files.length);
            if (res === 'cancel') { input.value = ''; clearFinalPreview(input); return; }
            out.push(res === 'keep' ? f : res);
        }
        var dt = new DataTransfer();
        out.forEach(function (f) { dt.items.add(f); });
        input.dataset.gbCropDone = '1';
        input.files = dt.files;
        renderFinalPreview(input, out);
    }

    document.addEventListener('change', function (e) {
        var input = e.target;
        if (!(input instanceof HTMLInputElement) || !isImageInput(input)) { return; }
        if (input.dataset.gbCropDone === '1') { delete input.dataset.gbCropDone; return; }
        if (!input.files || input.files.length === 0 || typeof DataTransfer === 'undefined') { clearFinalPreview(input); return; }
        process(input);
    }, true);
})();
