using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaVenta.Entity
{
    public class Pago
    {
        public int PagoID { get; set; }             // ID del pago
        public int FacturaID { get; set; }          // ID de la factura
        public decimal MontoPagado { get; set; }    // Monto pagado
        public DateTime FechaPago { get; set; }     // Fecha del pago
        public decimal? Recargo { get; set; }       // Recargo opcional

        // Relación con Factura
        public Factura Factura { get; set; }        // Factura relacionada
    }

}
