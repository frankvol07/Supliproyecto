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

        // Campos para desglose de efectivo
        [Range(0, int.MaxValue, ErrorMessage = "La cantidad debe ser positiva.")]
        public int? Monto2000 { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "La cantidad debe ser positiva.")]
        public int? Monto1000 { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "La cantidad debe ser positiva.")]
        public int? Monto500 { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "La cantidad debe ser positiva.")]
        public int? Monto200 { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "La cantidad debe ser positiva.")]
        public int? Monto100 { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "La cantidad debe ser positiva.")]
        public int? Monto50 { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "La cantidad debe ser positiva.")]
        public int? Monto25 { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "La cantidad debe ser positiva.")]
        public int? Monto10 { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "La cantidad debe ser positiva.")]
        public int? Monto5 { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "La cantidad debe ser positiva.")]
        public int? Monto1 { get; set; }

        // Nuevos campos
        public decimal? TotalDesglose { get; set; }

        [Range(-double.MaxValue, double.MaxValue, ErrorMessage = "El valor de la diferencia es inválido.")]
        public decimal? Diferencia { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "La ganancia debe ser positiva.")]
        public decimal? Ganancia { get; set; }
    }
}
