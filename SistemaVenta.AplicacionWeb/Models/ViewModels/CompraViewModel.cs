namespace SistemaVenta.AplicacionWeb.Models.ViewModels
{
    public class CompraViewModel
    {
          public int CompraId { get; set; }
        public string NombreProveedor { get; set; }
        public string NombreProducto { get; set; }
        public decimal Costo { get; set; }
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }

        public string Descripcion { get; set; }
        public string FormaPago { get; set; }
        public string NoTarjeta { get; set; }
        public string NoCuenta { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal MontoRestante { get; set; }
        public string NoComprobanteFiscal { get; set; }
    }

}
