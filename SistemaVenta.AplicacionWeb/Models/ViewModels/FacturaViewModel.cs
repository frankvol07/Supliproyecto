using System.ComponentModel.DataAnnotations;

namespace SistemaVenta.AplicacionWeb.Models.ViewModels
{
    public class FacturaViewModel
    {
        [Required]
        public int ClienteId { get; set; } // ID del cliente

        [Required]
        public decimal MontoTotal { get; set; } // Monto total de la factura
        public int FacturaID { get; set; }
        public string NumeroIdentificacion { get; set; }
        public string NombreCliente { get; set; }
        public decimal Abono { get; set; }
        public decimal Pendiente { get; set; }

        public string Estado { get; set; }
    }
}
