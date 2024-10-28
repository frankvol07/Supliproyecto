using System.ComponentModel.DataAnnotations;

namespace SistemaVenta.AplicacionWeb.Models.ViewModels
{
    public class CxCobrarViewModel
    {
        [Required]
        public int ClienteId { get; set; } // ID del cliente

        [Required]
        public decimal MontoTotal { get; set; } // Monto total de la factura


        public bool FacturaCredito { get; set; }

    }
}
