(function () {
    function init() {
        var same = document.getElementById('Input_BillingSameAsDelivery');
        var block = document.querySelector('[data-gb-billing]');
        if (!same || !block) return;
        function sync() { block.hidden = same.checked; }
        same.addEventListener('change', sync);
        sync();
    }
    init();
    document.addEventListener('gb:content-replaced', init);
})();
