document.addEventListener('DOMContentLoaded', function () {
    var form = document.getElementById('abrirCajaForm');

    form.addEventListener('submit', function (event) {
        var fechaApertura = document.getElementById('FechaApertura').value;
        var montoInicial = document.getElementById('MontoInicial').value;

        var valid = true;

        // Validar que la fecha de apertura no esté vacía
        if (!fechaApertura) {
            alert('Debe ingresar una fecha de apertura.');
            valid = false;
        }

        // Validar que el monto inicial no esté vacío ni sea negativo
        if (!montoInicial || montoInicial <= 0) {
            alert('Debe ingresar un monto inicial válido.');
            valid = false;
        }

        // Si no es válido, evita el envío del formulario
        if (!valid) {
            event.preventDefault();
        }
    });
});

