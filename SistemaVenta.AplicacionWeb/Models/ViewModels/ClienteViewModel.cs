using System.ComponentModel.DataAnnotations;

namespace SistemaVenta.AplicacionWeb.Models.ViewModels
{
    public class ClienteViewModel
    {
        public int ClienteId { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "Los apellidos son obligatorios")]
        [StringLength(100, ErrorMessage = "Los apellidos no pueden superar los 100 caracteres")]
        public string Apellidos { get; set; }

        [Required(ErrorMessage = "El tipo de identificación es obligatorio")]
        public string TipoIdentificacion { get; set; }

        [Required(ErrorMessage = "El número de identificación es obligatorio")]
        [StringLength(20, ErrorMessage = "El número de identificación no puede superar los 20 caracteres")]
        public string NumeroIdentificacion { get; set; }

        [Phone(ErrorMessage = "Ingrese un número de teléfono válido")]
        [StringLength(15, ErrorMessage = "El número de teléfono no puede superar los 15 caracteres")]
        public string NumeroTelefono { get; set; }

        [Phone(ErrorMessage = "Ingrese un número de celular válido")]
        [StringLength(15, ErrorMessage = "El número de celular no puede superar los 15 caracteres")]
        public string NumeroCelular { get; set; }

        [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido")]
        [StringLength(100, ErrorMessage = "El correo no puede superar los 100 caracteres")]
        public string Correo { get; set; }

        [StringLength(255, ErrorMessage = "La dirección no puede superar los 255 caracteres")]
        public string Direccion { get; set; }
    }

}
