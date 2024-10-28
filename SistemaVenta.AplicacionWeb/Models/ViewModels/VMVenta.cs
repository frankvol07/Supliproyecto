using SistemaVenta.Entity;

namespace SistemaVenta.AplicacionWeb.Models.ViewModels
{
    public class VMVenta
    {
        public int IdVenta { get; set; }
        public string? NumeroVenta { get; set; }
        public int? IdTipoDocumentoVenta { get; set; }
        public string? TipoDocumentoVenta { get; set; }
        public int? IdUsuario { get; set; }
        public string? Usuario { get; set; }
        public string? DocumentoCliente { get; set; }
        public string? NombreCliente { get; set; }
        public string? SubTotal { get; set; }
        public decimal? ImpuestoTotal { get; set; }
        public string? Total { get; set; }
        public string? FechaRegistro { get; set; }

        // Propiedades adicionales
        public int IdFactura { get; set; }  // Propiedad IdFactura
        public int IdCliente { get; set; }  // Propiedad IdCliente
        public DateTime FechaVencimiento { get; set; }  // Propiedad FechaVencimiento
        public decimal MontoTotal { get; set; }  // Propiedad MontoTotal (si la usas aquí)

        public virtual ICollection<VMDetalleVenta> DetalleVenta { get; set; }

        // Relación con Factura
        public FacturaViewModel Factura { get; set; }
    }
}
