using System.ComponentModel.DataAnnotations;

namespace SistemaVenta.AplicacionWeb.Models.ViewModels
{
    public class CajaViewModel
    {
        public int CajaId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime FechaApertura { get; set; }

        [DataType(DataType.Date)]
        public DateTime? FechaCierre { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "El monto inicial debe ser positivo.")]
        public decimal MontoInicial { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El monto final debe ser positivo.")]
        public decimal? MontoFinal { get; set; }
        public bool Estado { get; set; }
        public bool IsMontoInicialEditable { get; set; }
    }

}
