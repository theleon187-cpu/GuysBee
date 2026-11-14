(function () {
    var input = document.getElementById('gb-quick-filter-input');
    if (!input) return;
    var empty = document.getElementById('gb-quick-filter-empty');
    var checks = {
        marche: document.getElementById('gb-filter-marche'),
        commerce: document.getElementById('gb-filter-commerce')
    };
    var norm = function (s) {
        return (s || '').toLowerCase().normalize('NFD').replace(/[\u0300-\u036f]/g, '');
    };
    var data = Array.prototype.slice.call(document.querySelectorAll('[data-gb-group]')).map(function (g) {
        var items = Array.prototype.slice.call(g.querySelectorAll('.gb-agenda-item'));
        return { group: g, type: g.getAttribute('data-gb-group'), items: items, texts: items.map(function (li) { return norm(li.textContent); }) };
    });
    function apply() {
        var q = norm(input.value.trim());
        var visible = 0;
        data.forEach(function (d) {
            var typeOn = !checks[d.type] || checks[d.type].checked;
            var count = 0;
            d.items.forEach(function (li, i) {
                var show = typeOn && (!q || d.texts[i].indexOf(q) !== -1);
                li.hidden = !show;
                if (show) count++;
            });
            d.group.hidden = count === 0 && (!!q || !typeOn);
            visible += count;
        });
        if (empty) empty.hidden = visible > 0;
        document.dispatchEvent(new CustomEvent('gb:filter', { detail: {
            q: input.value.trim(),
            marche: !checks.marche || checks.marche.checked,
            commerce: !checks.commerce || checks.commerce.checked
        } }));
    }
    input.addEventListener('input', apply);
    Object.keys(checks).forEach(function (k) { if (checks[k]) checks[k].addEventListener('change', apply); });
})();
