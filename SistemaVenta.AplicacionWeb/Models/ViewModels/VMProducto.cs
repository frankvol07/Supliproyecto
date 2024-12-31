namespace SistemaVenta.AplicacionWeb.Models.ViewModels
{
    public class VMProducto
    {
        public int IdProducto { get; set; }
        public string? CodigoBarra { get; set; }
        public string? Marca { get; set; }
        public string? Descripcion { get; set; }
        public int? IdCategoria { get; set; }
        public string? NombreCategoria { get; set; }
        public int? Stock { get; set; }
        public string? UrlImagen { get; set; }
        public string? Precio { get; set; }
        public int? EsActivo { get; set; }

        // Nuevos campos agregados en el ViewModel
        public string? Tipo_Venta { get; set; }          // Tipo de venta
        public string? Presentacion { get; set; }        // Presentación
        public decimal? Costo { get; set; }              // Costo del producto
        public decimal? Porcentaje_Ganancia { get; set; } // Porcentaje de ganancia
        public decimal? Ganancia { get; set; }           // Ganancia en términos monetarios
       // Precio de venta final
    }
}
