(function () {
    var input = document.getElementById('gb-quick-filter-input');
    if (!input) return;
    var empty = document.getElementById('gb-quick-filter-empty');
    var items = Array.prototype.slice.call(document.querySelectorAll('.gb-agenda-item'));
    var norm = function (s) {
        return (s || '').toLowerCase().normalize('NFD').replace(/[\u0300-\u036f]/g, '');
    };
    var texts = items.map(function (li) { return norm(li.textContent); });
    input.addEventListener('input', function () {
        var q = norm(input.value.trim());
        var visible = 0;
        items.forEach(function (li, i) {
            var show = !q || texts[i].indexOf(q) !== -1;
            li.hidden = !show;
            if (show) visible++;
        });
        if (empty) empty.hidden = visible > 0;
    });
})();
