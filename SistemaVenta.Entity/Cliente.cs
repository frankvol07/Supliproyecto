using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaVenta.Entity
{
    public class Cliente
    {
        public int ClienteId { get; set; }           // ID del cliente
        public string Nombre { get; set; }           // Nombre
        public string Apellidos { get; set; }        // Apellidos
        public string TipoIdentificacion { get; set; } // Tipo de Identificación (Cédula/RNC)
        public string NumeroIdentificacion { get; set; } // Número de Identificación
        public string NumeroTelefono { get; set; }   // Número de Teléfono
        public string NumeroCelular { get; set; }    // Número de Celular
        public string Correo { get; set; }           // Correo Electrónico
        public string Direccion { get; set; }
        public decimal? CreditoDisponible { get; set; }  // Nullable por si el valor es opcional
        public decimal? LimiteCredito { get; set; }      // Nullable por si el valor es opcional

        // Relación con Facturas
        public ICollection<Factura> Facturas { get; set; }
        // Dirección
    }
}
