using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SistemaVenta.AplicacionWeb.Models.ViewModels
{
    public class ClienteViewModel
    {
        // Propiedades del Cliente
        public int ClienteId { get; set; }

        public string Nombre { get; set; }


        public string Apellidos { get; set; }


        public string TipoIdentificacion { get; set; }


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


        public string Direccion { get; set; }

        public decimal? CreditoDisponible { get; set; }
        public decimal? LimiteCredito { get; set; }

        // Propiedades de la Factura

        public DateTime FechaEmision { get; set; }

        public DateTime? FechaVencimiento { get; set; }


        public decimal MontoTotal { get; set; }


        public string Estado { get; set; } = "Pendiente"; // Estado por defecto

        // Propiedades del Pago

        public decimal MontoPagado { get; set; }


        public DateTime FechaPago { get; set; }

        public decimal? Recargo { get; set; } // Recargo opcional
    }
}
