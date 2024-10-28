using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaVenta.Entity
{
    public class FacturaProveedor
    {
        [Key]
        public int FacturaProveedorID { get; set; }
        public int ProveedorID { get; set; }
        public DateTime FechaEmision { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public decimal MontoTotal { get; set; }
        public string Detalle { get; set; }
        public string Estado { get; set; }

        // Este campo no será utilizado en la inserción de datos
        public decimal? Abono { get; set; }
    }






}
