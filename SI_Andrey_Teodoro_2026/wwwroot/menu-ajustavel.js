(function () {
    const chave = 'menuLargura';
    const minimo = 200;
    const maximo = 480;
    let estilo = null;

    function aplicar(largura) {
        const valor = Math.min(maximo, Math.max(minimo, Math.round(largura)));
        if (!estilo) {
            estilo = document.createElement('style');
            document.head.appendChild(estilo);
        }
        estilo.textContent = `.mud-layout{--mud-drawer-width-left:${valor}px !important}.mud-drawer.mud-drawer-pos-left{--mud-drawer-width:${valor}px !important}`;
        return valor;
    }

    function salvar(valor) {
        try { localStorage.setItem(chave, String(valor)); } catch (e) { }
    }

    try {
        const salvo = parseInt(localStorage.getItem(chave), 10);
        if (!isNaN(salvo)) aplicar(salvo);
    } catch (e) { }

    document.addEventListener('mousedown', function (e) {
        if (e.button !== 0 || !e.target.closest('.menu-resize')) return;
        e.preventDefault();
        const drawer = document.querySelector('.mud-drawer.mud-drawer-pos-left');
        const esquerda = drawer ? drawer.getBoundingClientRect().left : 0;
        let atual = null;
        document.body.classList.add('menu-redimensionando');

        function mover(ev) { atual = aplicar(ev.clientX - esquerda); }
        function soltar() {
            document.removeEventListener('mousemove', mover);
            document.removeEventListener('mouseup', soltar);
            document.body.classList.remove('menu-redimensionando');
            if (atual !== null) salvar(atual);
        }

        document.addEventListener('mousemove', mover);
        document.addEventListener('mouseup', soltar);
    });
})();
