using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaVenta.Entity
{
    public class VentaRNC
    {
        public int Id { get; set; }
        [Column("RNC_Cedula")]
        public string RncCedula { get; set; }             // RNC o cédula del cliente
        public string TipoId { get; set; }                // Tipo de identificación (RNC o Cédula)
        public string BienServicioComprado { get; set; }  // Bien o servicio comprado
        public string Ncf { get; set; }                   // NCF utilizado
        public DateTime FechaComprobante { get; set; }    // Fecha del comprobante
        public DateTime FechaPago { get; set; }           // Fecha del pago
        public decimal TotalMontoFacturado { get; set; }  // Total monto facturado
        public decimal ItbisFacturado { get; set; }       // ITBIS facturado (IGV)
    }

}
