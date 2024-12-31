// Escuchar cambios en el selector de rango de tiempo
document.getElementById("rangoTiempo").addEventListener("change", function () {
    let rangoSeleccionado = this.value;
    obtenerDatosPorRango(rangoSeleccionado);
});

// Mantener referencia de los gráficos para poder destruirlos
let graficos = {};

// Función para obtener datos en base al rango seleccionado
function obtenerDatosPorRango(rango) {
    $("div.container-fluid").LoadingOverlay("show"); // Mostrar un indicador de carga
    fetch("/DashBoard/ObtenerResumen?Rango=" + rango)
        .then(response => {
            $("div.container-fluid").LoadingOverlay("hide");
            return response.ok ? response.json() : Promise.reject(response);
        })
        .then(responseJson => {
            console.log("Datos obtenidos:", responseJson); // Verifica los datos obtenidos
            if (responseJson.estado) {
                let d = responseJson.objeto;

                // Actualizar datos de las tarjetas
                $("#totalVenta").text(d.totalVentas);
                $("#totalIngresos").text(d.totalIngresos);
                $("#totalProductos").text(d.totalProductos);
                $("#totalCategorias").text(d.totalCategorias);

                // Preparar datos para los gráficos
                let barchart_labels = d.ventasUltimaSemana.length > 0 ? d.ventasUltimaSemana.map(item => item.fecha) : ["sin resultados"];
                let barchart_data = d.ventasUltimaSemana.length > 0 ? d.ventasUltimaSemana.map(item => item.total) : [0];

                let piechart_labels = d.productosTopUltimaSemana.length > 0 ? d.productosTopUltimaSemana.map(item => item.producto) : ["sin resultados"];
                let piechart_data = d.productosTopUltimaSemana.length > 0 ? d.productosTopUltimaSemana.map(item => item.cantidad) : [0];

                // Actualizar gráficos
                actualizarGrafico("chartVentas", "bar", barchart_labels, barchart_data);
                actualizarGrafico("chartProductos", "doughnut", piechart_labels, piechart_data);
            }
        })
        .catch(error => {
            console.error("Error al obtener datos:", error);
        });
}

// Función para actualizar o redibujar gráficos
function actualizarGrafico(id, tipo, labels, data) {
    // Si ya existe un gráfico, destruirlo antes de crear uno nuevo
    if (graficos[id]) {
        graficos[id].destroy();
    }

    // Crear una nueva instancia del gráfico
    let ctx = document.getElementById(id).getContext("2d");
    graficos[id] = new Chart(ctx, {
        type: tipo,
        data: {
            labels: labels,
            datasets: [{
                data: data,
                backgroundColor: tipo === "bar" ? "#4e73df" : ["#4e73df", "#1cc88a", "#36b9cc", "#FF785B"],
                hoverBackgroundColor: tipo === "bar" ? "#2e59d9" : ["#2e59d9", "#17a673", "#2c9faf", "#FF5733"],
                hoverBorderColor: "rgba(234, 236, 244, 1)",
            }],
        },
        options: {
            maintainAspectRatio: false,
            responsive: true,
            plugins: {
                tooltip: {
                    backgroundColor: "rgb(255,255,255)",
                    bodyColor: "#858796",
                    borderColor: '#dddfeb',
                    borderWidth: 1,
                    displayColors: false,
                },
                legend: { display: true },
            },
            cutoutPercentage: tipo === "doughnut" ? 80 : 0,
        },
    });
}

// Llamar a la función al cargar la página con el rango predeterminado
document.addEventListener("DOMContentLoaded", function () {
    obtenerDatosPorRango(document.getElementById("rangoTiempo").value);
});
