// Aperçu immédiat de toute photo choisie dans un champ fichier de l'administration.
document.addEventListener('change', function (e) {
    var input = e.target;
    if (!(input instanceof HTMLInputElement) || input.type !== 'file') { return; }
    if (!(input.accept || '').match(/image|jpg|jpeg|png|webp/i)) { return; }

    var holder = input.parentElement.querySelector('.gb-admin-preview-list[data-for="' + (input.id || '') + '"]');
    if (!holder) {
        holder = document.createElement('div');
        holder.className = 'gb-admin-preview-list';
        holder.setAttribute('data-for', input.id || '');
        input.insertAdjacentElement('afterend', holder);
    }
    holder.querySelectorAll('img').forEach(function (img) { URL.revokeObjectURL(img.src); });
    holder.textContent = '';

    Array.prototype.forEach.call(input.files || [], function (file) {
        if (!file.type.startsWith('image/')) { return; }
        var img = document.createElement('img');
        img.className = 'gb-admin-preview';
        img.alt = 'Aperçu de ' + file.name;
        img.src = URL.createObjectURL(file);
        holder.appendChild(img);
    });
});
