using SistemaVenta.Entity;

namespace SistemaVenta.AplicacionWeb.Models.ViewModels
{
    public class FacturaProveedorViewModel
    {
        // Para la inserción
        public int ProveedorID { get; set; }
        public DateTime FechaEmision { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public decimal MontoTotal { get; set; }
        public string Detalle { get; set; }
        public string Estado { get; set; }

        // Para la visualización
        public List<FacturaProveedor> Facturas { get; set; } = new List<FacturaProveedor>(); // Inicializa la lista
    }
}
