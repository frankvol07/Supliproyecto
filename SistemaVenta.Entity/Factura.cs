using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaVenta.Entity
{
    public class Factura
    {
        public int FacturaID { get; set; }           // ID de la factura
        public int ClienteID { get; set; }           // ID del cliente
        public DateTime FechaEmision { get; set; }   // Fecha de emisión
        public DateTime? FechaVencimiento { get; set; } // Fecha de vencimiento
        public decimal MontoTotal { get; set; }      // Monto total
        public string Estado { get; set; }           // Estado de la factura
        public decimal? Abono { get; set; }

        public Cliente Cliente { get; set; }          // Cliente relacionado

        // Relación con Pagos
        public ICollection<Pago> Pagos { get; set; }  // Lista de pagos

        [NotMapped] // Asegúrate de marcar esta propiedad como no mapeada
        public decimal Pendiente => MontoTotal - (Abono ?? 0);
    }

}