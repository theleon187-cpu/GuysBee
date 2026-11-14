// Carte unique des marchés et commerces (Leaflet + OpenStreetMap).
document.addEventListener('DOMContentLoaded', function () {
    var el = document.getElementById('gb-points-map');
    var data = document.getElementById('gb-points-data');
    if (!el || !data || typeof L === 'undefined') { return; }

    var points = JSON.parse(data.textContent || '[]');
    if (!points.length) { return; }

    var map = L.map(el, { scrollWheelZoom: false });
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 18,
        attribution: '&copy; contributeurs <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
    }).addTo(map);

    function line(parent, cls, text) {
        if (!text) { return; }
        var p = document.createElement('p');
        p.className = cls;
        p.textContent = text;
        parent.appendChild(p);
    }

    var bounds = [];
    var markers = [];
    points.forEach(function (pt) {
        var popup = document.createElement('div');
        popup.className = 'gb-map-popup';
        var type = document.createElement('p');
        type.className = 'gb-map-popup-type';
        type.textContent = pt.kind === 'marche' ? 'Marché' : 'Commerce';
        popup.appendChild(type);
        var title = document.createElement('strong');
        title.textContent = pt.name;
        popup.appendChild(title);
        line(popup, 'gb-map-popup-when', pt.when);
        line(popup, 'gb-map-popup-hours', pt.hours);
        line(popup, 'gb-map-popup-place', pt.place);
        line(popup, 'gb-map-popup-details', pt.details);
        if (pt.link) {
            var a = document.createElement('a');
            a.href = pt.link;
            a.target = '_blank';
            a.rel = 'noopener';
            a.textContent = 'Itinéraire';
            popup.appendChild(a);
        }

        var marker = L.circleMarker([pt.lat, pt.lng], {
            radius: 10,
            className: 'gb-map-marker gb-map-marker-' + pt.kind
        }).bindPopup(popup).bindTooltip(pt.name).addTo(map);
        markers.push({ pt: pt, marker: marker });
        bounds.push([pt.lat, pt.lng]);
    });

    var norm = function (s) {
        return (s || '').toLowerCase().normalize('NFD').replace(/[\u0300-\u036f]/g, '');
    };
    document.addEventListener('gb:filter', function (ev) {
        var f = ev.detail || {};
        var q = norm(f.q || '');
        var shown = [];
        markers.forEach(function (m) {
            var typeOn = m.pt.kind === 'marche' ? f.marche !== false : f.commerce !== false;
            var text = norm([m.pt.name, m.pt.when, m.pt.hours, m.pt.place, m.pt.details].join(' '));
            var show = typeOn && (!q || text.indexOf(q) !== -1);
            if (show) { m.marker.addTo(map); shown.push([m.pt.lat, m.pt.lng]); }
            else { m.marker.remove(); }
        });
        if (shown.length === 1) { map.setView(shown[0], 13); }
        else if (shown.length > 1) { map.fitBounds(shown, { padding: [30, 30] }); }
    });

    if (bounds.length === 1) { map.setView(bounds[0], 13); }
    else { map.fitBounds(bounds, { padding: [30, 30] }); }

    el.addEventListener('click', function () { map.scrollWheelZoom.enable(); }, { once: true });
});
