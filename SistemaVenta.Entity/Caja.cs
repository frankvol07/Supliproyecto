using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaVenta.Entity
{
    public class Caja
    {
        public int CajaId { get; set; }
        public DateTime FechaApertura { get; set; }
        public DateTime? FechaCierre { get; set; }
        public decimal MontoInicial { get; set; }
        public decimal? MontoFinal { get; set; }
        public bool Estado { get; set; } // Por ejemplo, "Abierta" o "Cerrada"

        // Campos para desglose de efectivo
        public int? Monto2000 { get; set; }
        public int? Monto1000 { get; set; }
        public int? Monto500 { get; set; }
        public int? Monto200 { get; set; }
        public int? Monto100 { get; set; }
        public int? Monto50 { get; set; }
        public int? Monto25 { get; set; }
        public int? Monto10 { get; set; }
        public int? Monto5 { get; set; }
        public int? Monto1 { get; set; }

        // Nuevos campos
        public decimal? TotalDesglose { get; set; }
        public decimal? Diferencia { get; set; }
        public decimal? Ganancia { get; set; }
    }
}
