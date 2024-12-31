using System;
using System.Collections.Generic;

namespace SistemaVenta.Entity
{
    public partial class Producto
    {
        public int IdProducto { get; set; }
        public string? CodigoBarra { get; set; }
        public string? Marca { get; set; }
        public string? Descripcion { get; set; }
        public int? IdCategoria { get; set; }
        public int? Stock { get; set; }
        public string? UrlImagen { get; set; }
        public string? NombreImagen { get; set; }
        public decimal? Precio { get; set; }
        public bool? EsActivo { get; set; }
        public DateTime? FechaRegistro { get; set; }

        // Nuevos campos agregados
        public string? Tipo_Venta { get; set; }          // Tipo de venta
        public string? Presentacion { get; set; }        // Presentación
        public decimal? Costo { get; set; }              // Costo del producto
        public decimal? Porcentaje_Ganancia { get; set; } // Porcentaje de ganancia
        public decimal? Ganancia { get; set; }           // Ganancia en términos monetarios
       // Precio de venta final

        public virtual Categoria? IdCategoriaNavigation { get; set; }
    }
}

