using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaVenta.Entity
{
    public class DetalleTransaccion607
    {
        [Key]  // Anotación para marcar 'IdDetalle' como la clave primaria
        public int IdDetalle { get; set; }

        public string RncCedulaCliente { get; set; }
        public string TipoIdentificacion { get; set; }
        public string Ncf { get; set; }
        public DateTime FechaComprobante { get; set; }
        public decimal? ItbisFacturado { get; set; }
        public decimal MontoFacturado { get; set; }
    }
}
