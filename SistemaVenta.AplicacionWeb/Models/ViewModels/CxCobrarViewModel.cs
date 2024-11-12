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
    public class FacturaViewModels
    {
        public string clienteId { get; set; }
        public string nombreCliente { get; set; }
        public string documentoCliente { get; set; }
        public string tipoDocumento { get; set; }
        public string subTotal { get; set; }
        public string igv { get; set; }
        public string total { get; set; }
        public decimal MontoPagar { get; set; }  // Ya existente, según contexto
        public decimal montoRestante { get; set; }
        public string NCF { get; set; }

        public List<ProductoViewModel> productos { get; set; }
    }

    public class ProductoViewModel
    {
        public string nombreProducto { get; set; }
        public string cantidad { get; set; }
        public string precio { get; set; }
        public string total { get; set; }
    }
}
