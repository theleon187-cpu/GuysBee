(function () {
    var form = document.getElementById('gb-findus-form');
    var plan = document.getElementById('gb-plan');
    if (!form || !plan) return;

    var el = function (n) { return form.querySelector('[name="' + n + '"]:not([type="hidden"])'); };
    var block = function (n) { return plan.querySelector('[data-block="' + n + '"]'); };
    var marketsFirst = document.getElementById('MarketsFirst');

    function checked(name) { var i = el(name); return !!(i && i.checked); }
    function radio(name) { var r = form.querySelector('input[name="' + name + '"]:checked'); return r ? r.value : ''; }

    function render() {
        var pos = radio('MapPosition') || 'dessus';
        plan.className = 'gb-plan gb-plan-map-' + pos;
        block('filter').hidden = !checked('ShowFilter');
        block('map').hidden = !checked('ShowMap');
        plan.querySelector('.gb-plan-lists').classList.toggle('gb-plan-reverse', marketsFirst.value !== 'true');
        block('markets').querySelector('.gb-plan-name').textContent = (form.querySelector('#MarketsTitle').value || 'Marchés');
        block('shops').querySelector('.gb-plan-name').textContent = (form.querySelector('#ShopsTitle').value || 'Commerces');
        var layout = radio('Layout') || 'liste';
        plan.setAttribute('data-layout', layout);
    }

    function setOrder(first) {
        marketsFirst.value = first ? 'true' : 'false'; render();
    }

    document.getElementById('gb-plan-up').addEventListener('click', function () { setOrder(true); });
    document.getElementById('gb-plan-down').addEventListener('click', function () { setOrder(false); });
    form.addEventListener('input', render);
    form.addEventListener('change', render);
    render();
})();
