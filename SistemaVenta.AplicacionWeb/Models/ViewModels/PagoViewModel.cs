namespace SistemaVenta.AplicacionWeb.Models.ViewModels
{
    public class PagoViewModel
    {
        public int PagoID { get; set; }
        public int FacturaID { get; set; }
        public decimal MontoPagado { get; set; }
        public DateTime FechaPago { get; set; }
        public decimal? Recargo { get; set; }
    }
}
