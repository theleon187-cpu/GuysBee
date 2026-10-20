(function () {
    var openButton = document.getElementById('gb-relay-open');
    if (!openButton || !window.jQuery) return;

    function setValue(id, value) {
        var field = document.getElementById(id);
        if (field) field.value = value || '';
    }

    function selectRelayMethod() {
        var field = document.getElementById('Input_RelayName');
        var radio = field && field.closest('.gb-relay') && field.closest('.gb-relay').previousElementSibling
            ? field.closest('.gb-relay').previousElementSibling.querySelector('input[type=radio]')
            : null;
        if (radio) radio.checked = true;
    }

    window.gbRelayChosen = function (point) {
        setValue('Input_RelayName', point.nom);
        setValue('Input_RelayAddress', [point.adresse1, point.codePostal, point.localite].filter(Boolean).join(', '));
        selectRelayMethod();
        window.jQuery('#gb-relay-open').frameColissimoClose();
    };

    openButton.addEventListener('click', function () {
        fetch(openButton.getAttribute('data-relay-token-url'), { headers: { Accept: 'application/json' } })
            .then(function (response) { if (!response.ok) throw new Error(); return response.json(); })
            .then(function (data) {
                var zip = document.getElementById('Input_PostalCode');
                var city = document.getElementById('Input_City');
                var address = document.getElementById('Input_AddressLine');
                window.jQuery('#gb-relay-open').frameColissimoOpen({
                    URLColissimo: 'https://ws.colissimo.fr',
                    callBackFrame: 'gbRelayChosen',
                    ceCountry: 'FR',
                    ceAddress: address ? address.value : '',
                    ceZipCode: zip ? zip.value : '',
                    ceTown: city ? city.value : '',
                    token: data.token,
                    filterRelay: '1'
                });
            })
            .catch(function () {
                window.alert("La carte des points relais est momentanément indisponible. Utilisez le lien de recherche et saisissez votre point relais.");
            });
    });
})();
