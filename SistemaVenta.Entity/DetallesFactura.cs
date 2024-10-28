using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaVenta.Entity
{
    public class DetallesFactura
    {
        
        [Key]
        public int DetalleID { get; set; }
        public int FacturaID { get; set; }
        public int ProductoID { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioTotal { get; set; }

        // Relación con Factura
        public Factura Factura { get; set; }

        // Relación con ProductoServicio
        public ProductoServicio ProductoServicio { get; set; }
    }

}
