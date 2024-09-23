using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SistemaVenta.AplicacionWeb.Models.ViewModels
{
    public class ProveedorViewModel
    {
        // Datos del proveedor para agregar/editar
        public int IdProveedor { get; set; }

        [Required(ErrorMessage = "La identificación es requerida")]
        [StringLength(50)]
        public string Identificacion { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El correo es requerido")]
        [EmailAddress(ErrorMessage = "Correo no válido")]
        [StringLength(100)]
        public string Correo { get; set; }

        [Required(ErrorMessage = "El teléfono es requerido")]
        [StringLength(15)]
        public string Telefono { get; set; }

        [Required(ErrorMessage = "La dirección es requerida")]
        [StringLength(200)]
        public string Direccion { get; set; }

        // Lista de proveedores para mostrar en la tabla
        public List<ProveedorViewModel> ProveedoresList { get; set; } = new List<ProveedorViewModel>();
    }
}
