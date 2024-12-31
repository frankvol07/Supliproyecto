using Microsoft.AspNetCore.Mvc;
using SistemaVenta.Entity;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using SistemaVenta.DAL.DBContext;
using System.Text;

namespace SistemaVenta.AplicacionWeb.Controllers
{
    public class CuadreController : Controller
    {
        private readonly DBVENTAContext _context;

        public CuadreController(DBVENTAContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GenerarReporte(DateTime diaSeleccionado)
        {
            try
            {
                // Filtrar cajas por la fecha seleccionada (solo fecha, ignorando hora)
                var cajas = _context.Cajas
                    .Where(c => c.FechaApertura.Date == diaSeleccionado.Date)
                    .ToList();

                if (!cajas.Any())
                {
                    return NotFound("No hay registros para el día seleccionado.");
                }

                // Crear documento PDF
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    Document pdfDoc = new Document(PageSize.A4, 20, 20, 20, 20); // Márgenes ajustados
                    PdfWriter writer = PdfWriter.GetInstance(pdfDoc, memoryStream);

                    pdfDoc.Open();

                    // Encabezado del reporte
                    var fontTitle = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                    var fontSubTitle = FontFactory.GetFont(FontFactory.HELVETICA, 12);
                    var fontBody = FontFactory.GetFont(FontFactory.HELVETICA, 10);

                    pdfDoc.Add(new Paragraph("Reporte de Cuadre de Caja", fontTitle));
                    pdfDoc.Add(new Paragraph($"Fecha: {diaSeleccionado:dd/MM/yyyy}", fontSubTitle));
                    pdfDoc.Add(new Paragraph("\n"));

                    // Tabla principal
                    PdfPTable mainTable = new PdfPTable(7) { WidthPercentage = 100 };
                    float[] mainTableWidths = { 1.5f, 1.5f, 1.5f, 1.5f, 1.5f, 2f, 2f }; // Anchos relativos
                    mainTable.SetWidths(mainTableWidths);

                    // Cabeceras de la tabla principal
                    BaseColor turquesa = new BaseColor(64, 224, 208); // Color turquesa
                    mainTable.AddCell(new PdfPCell(new Phrase("Monto Inicial", fontBody)) { BackgroundColor = turquesa });
                    mainTable.AddCell(new PdfPCell(new Phrase("Monto Final", fontBody)) { BackgroundColor = turquesa });
                    mainTable.AddCell(new PdfPCell(new Phrase("Total Desglose", fontBody)) { BackgroundColor = turquesa });
                    mainTable.AddCell(new PdfPCell(new Phrase("Diferencia", fontBody)) { BackgroundColor = turquesa });
                    mainTable.AddCell(new PdfPCell(new Phrase("Ganancia", fontBody)) { BackgroundColor = turquesa });
                    mainTable.AddCell(new PdfPCell(new Phrase("Fecha Apertura", fontBody)) { BackgroundColor = turquesa });
                    mainTable.AddCell(new PdfPCell(new Phrase("Fecha Cierre", fontBody)) { BackgroundColor = turquesa });

                    // Filas principales y tablas de desglose
                    foreach (var caja in cajas)
                    {
                        mainTable.AddCell(caja.MontoInicial.ToString("C"));
                        mainTable.AddCell(caja.MontoFinal.HasValue ? caja.MontoFinal.Value.ToString("C") : "N/A");
                        mainTable.AddCell(caja.TotalDesglose.HasValue ? caja.TotalDesglose.Value.ToString("C") : "N/A");
                        mainTable.AddCell(caja.Diferencia.HasValue ? caja.Diferencia.Value.ToString("C") : "N/A");
                        mainTable.AddCell(caja.Ganancia.HasValue ? caja.Ganancia.Value.ToString("C") : "N/A");
                        mainTable.AddCell(caja.FechaApertura.ToString("dd/MM/yyyy HH:mm"));
                        mainTable.AddCell(caja.FechaCierre.HasValue ? caja.FechaCierre.Value.ToString("dd/MM/yyyy HH:mm") : "N/A");

                        // Añadir espaciado entre la tabla principal y la tabla de desglose
                        pdfDoc.Add(new Paragraph("\n")); // Espaciado

                        // Tabla secundaria: Desglose de montos
                        PdfPTable desgloseTable = new PdfPTable(2) { WidthPercentage = 80, SpacingBefore = 10 };
                        desgloseTable.AddCell(new PdfPCell(new Phrase("Denominación", fontBody)) { BackgroundColor = turquesa });
                        desgloseTable.AddCell(new PdfPCell(new Phrase("Cantidad", fontBody)) { BackgroundColor = turquesa });

                        if (caja.Monto2000.HasValue) { desgloseTable.AddCell("$2000"); desgloseTable.AddCell(caja.Monto2000.ToString()); }
                        if (caja.Monto1000.HasValue) { desgloseTable.AddCell("$1000"); desgloseTable.AddCell(caja.Monto1000.ToString()); }
                        if (caja.Monto500.HasValue) { desgloseTable.AddCell("$500"); desgloseTable.AddCell(caja.Monto500.ToString()); }
                        if (caja.Monto200.HasValue) { desgloseTable.AddCell("$200"); desgloseTable.AddCell(caja.Monto200.ToString()); }
                        if (caja.Monto100.HasValue) { desgloseTable.AddCell("$100"); desgloseTable.AddCell(caja.Monto100.ToString()); }
                        if (caja.Monto50.HasValue) { desgloseTable.AddCell("$50"); desgloseTable.AddCell(caja.Monto50.ToString()); }
                        if (caja.Monto25.HasValue) { desgloseTable.AddCell("$25"); desgloseTable.AddCell(caja.Monto25.ToString()); }
                        if (caja.Monto10.HasValue) { desgloseTable.AddCell("$10"); desgloseTable.AddCell(caja.Monto10.ToString()); }
                        if (caja.Monto5.HasValue) { desgloseTable.AddCell("$5"); desgloseTable.AddCell(caja.Monto5.ToString()); }
                        if (caja.Monto1.HasValue) { desgloseTable.AddCell("$1"); desgloseTable.AddCell(caja.Monto1.ToString()); }

                        // Agregar la tabla secundaria al documento
                        pdfDoc.Add(desgloseTable);
                    }

                    pdfDoc.Add(mainTable); // Agregar la tabla principal al documento
                    pdfDoc.Close();

                    return File(memoryStream.ToArray(), "application/pdf", "CuadreDeCaja.pdf");
                }
            }
            catch (Exception ex)
            {
                return BadRequest("Error al generar el reporte: " + ex.Message);
            }
        }


    }
}

