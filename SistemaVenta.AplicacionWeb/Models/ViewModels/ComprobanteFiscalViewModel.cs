using System.ComponentModel.DataAnnotations;

namespace SistemaVenta.AplicacionWeb.Models.ViewModels
{
    public class ComprobanteFiscalViewModel
    {
        [Required(ErrorMessage = "El tipo es requerido")]
        [StringLength(50, ErrorMessage = "El tipo no debe exceder los 50 caracteres")]
        public string Tipo { get; set; }

        [Required(ErrorMessage = "El campo NCF Desde es requerido")]
        [StringLength(20, ErrorMessage = "NCF Desde no debe exceder los 20 caracteres")]
        public string NCF_Desde { get; set; }

        [Required(ErrorMessage = "El campo NCF Hasta es requerido")]
        [StringLength(20, ErrorMessage = "NCF Hasta no debe exceder los 20 caracteres")]
        public string NCF_Hasta { get; set; }

        [Required(ErrorMessage = "El campo NCF Actual es requerido")]
        [StringLength(20, ErrorMessage = "NCF Actual no debe exceder los 20 caracteres")]
        public string NCF_Actual { get; set; }

        [Required(ErrorMessage = "El campo NCF Restan es requerido")]
        [Range(0, int.MaxValue, ErrorMessage = "NCF Restan debe ser un valor positivo")]
        public int NCF_Restan { get; set; }

        [Required(ErrorMessage = "La fecha de vencimiento es requerida")]
        [DataType(DataType.Date, ErrorMessage = "Fecha inválida")]
        public DateTime Fecha_Vto { get; set; }
    }
}
