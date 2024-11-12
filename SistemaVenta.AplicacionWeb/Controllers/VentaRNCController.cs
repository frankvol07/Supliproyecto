using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using SistemaVenta.DAL.DBContext;

namespace SistemaVenta.AplicacionWeb.Controllers
{
    public class VentaRNCController : Controller
    {
        private readonly DBVENTAContext _context;

        public VentaRNCController(DBVENTAContext context)
        {
            _context = context;
        }
        public ActionResult Index() { 
        
        return View();
        }
        public ActionResult ExportarExcel(DateTime fechaInicio, DateTime fechaFin)
        {
            var ventas = _context.VentaRNC
                .Where(v => v.FechaPago >= fechaInicio && v.FechaPago <= fechaFin)
                .ToList();

            if (!ventas.Any())
            {
                return Content("No hay datos para el rango de fechas seleccionadas.");
            }

            // Establecer el contexto de la licencia
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Crear una lista para el reporte exportado
            var exportList = ventas.Select(v => new
            {
                RncCedula = v.RncCedula,
                TipoId = v.TipoId,
                BienServicioComprado = v.BienServicioComprado,
                Ncf = v.Ncf,
                FechaComprobante = v.FechaComprobante.ToString("yyyy-MM-dd"), // Formato de fecha
                FechaPago = v.FechaPago.ToString("yyyy-MM-dd"), // Formato de fecha
                TotalMontoFacturado = v.TotalMontoFacturado,
                ItbisFacturado = v.ItbisFacturado
            }).ToList();

            using (ExcelPackage package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Ventas");
                worksheet.Cells["A1"].LoadFromCollection(exportList, true);
                worksheet.Cells.AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                var fileName = $"Ventas_{fechaInicio:yyyyMMdd}_{fechaFin:yyyyMMdd}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }
    }
}
