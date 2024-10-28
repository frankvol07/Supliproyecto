using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaVenta.Entity
{
    public class ProductoServicio
    {
        [Key]
        public int ProductoID { get; set; }
        public string Nombre { get; set; }
        public decimal PrecioUnitario { get; set; }
        public string Tipo { get; set; } // Ej: 'Producto' o 'Servicio'

        // Relación con DetallesFactura
        public ICollection<DetallesFactura> DetallesFacturas { get; set; }
    }
}
