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

        L.circleMarker([pt.lat, pt.lng], {
            radius: 10,
            className: 'gb-map-marker gb-map-marker-' + pt.kind
        }).bindPopup(popup).bindTooltip(pt.name).addTo(map);
        bounds.push([pt.lat, pt.lng]);
    });

    if (bounds.length === 1) { map.setView(bounds[0], 13); }
    else { map.fitBounds(bounds, { padding: [30, 30] }); }

    el.addEventListener('click', function () { map.scrollWheelZoom.enable(); }, { once: true });
});
