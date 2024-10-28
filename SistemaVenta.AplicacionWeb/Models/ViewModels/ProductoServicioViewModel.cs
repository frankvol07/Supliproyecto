using System.ComponentModel.DataAnnotations;

namespace SistemaVenta.AplicacionWeb.Models.ViewModels
{
    public class ProductoServicioViewModel
    {
    
        public int ProductoID { get; set; }
        public string Nombre { get; set; }
        public decimal PrecioUnitario { get; set; }
        public string Tipo { get; set; } // 'Producto' o 'Servicio'
    }

}
