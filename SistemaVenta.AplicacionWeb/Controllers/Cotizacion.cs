using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using SistemaVenta.DAL.DBContext;
using SistemaVenta.Entity;
using System.IO;
using System.Linq;

namespace SistemaVenta.AplicacionWeb.Controllers
{
    public class CotizacionController : Controller
    {
        private readonly DBVENTAContext _context;

        public CotizacionController(DBVENTAContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Pasar los clientes y productos a la vista para cargar los datos
            var clientes = _context.Clientes.ToList();
            var productos = _context.Productos.Where(p => p.EsActivo == true).ToList();

            ViewBag.Clientes = clientes;
            ViewBag.Productos = productos;

            return View();
        }

        [HttpPost]
        public IActionResult GenerarPdf(string nombre, string telefono, string email, string[] codigos, int[] cantidades, string[] descripciones, decimal[] preciosUnitarios, string condicionesPago, string validezOferta)
        {
            // Obtener los datos del negocio (asumimos que hay un único registro)
            Negocio negocio = _context.Negocios.FirstOrDefault();

            // Configuración del documento PDF
            Document pdfDoc = new Document(PageSize.A4, 25, 25, 30, 30);
            MemoryStream stream = new MemoryStream();
            PdfWriter.GetInstance(pdfDoc, stream).CloseStream = false;
            pdfDoc.Open();

            // Configuración de fuentes
            var titleFont = FontFactory.GetFont("Arial", 18, Font.BOLD);
            var boldFont = FontFactory.GetFont("Arial", 12, Font.BOLD);
            var regularFont = FontFactory.GetFont("Arial", 12);
            var smallFont = FontFactory.GetFont("Arial", 10);

            // Encabezado de la empresa (obtenido de la base de datos)
            if (negocio != null)
            {
                pdfDoc.Add(new Paragraph(negocio.Nombre ?? "Nombre de la Empresa", titleFont));
                pdfDoc.Add(new Paragraph("RNC: " + (negocio.NumeroDocumento ?? "RNC no disponible"), regularFont));
                pdfDoc.Add(new Paragraph("TEL.: " + (negocio.Telefono ?? "Teléfono no disponible"), regularFont));
                pdfDoc.Add(new Paragraph("Direccion: " + negocio.Direccion ?? "Dirección no disponible", regularFont));
                pdfDoc.Add(new Paragraph(" "));
            }

            // Información del cliente
            pdfDoc.Add(new Paragraph($"Cliente: {nombre}", boldFont));
            pdfDoc.Add(new Paragraph($"Teléfono: {telefono}", regularFont));
            pdfDoc.Add(new Paragraph($"Email: {email}", regularFont));
            pdfDoc.Add(new Paragraph(" "));

            // Tabla de productos/servicios
            PdfPTable table = new PdfPTable(4);
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 50f, 15f, 15f, 20f });

            table.AddCell(new PdfPCell(new Phrase("Descripción", boldFont)));
            table.AddCell(new PdfPCell(new Phrase("Cantidad", boldFont)));
            table.AddCell(new PdfPCell(new Phrase("Precio Unitario", boldFont)));
            table.AddCell(new PdfPCell(new Phrase("SubTotal", boldFont)));

            decimal totalGeneral = 0;

            for (int i = 0; i < codigos.Length; i++)
            {
                decimal precioConITBIS = preciosUnitarios[i] * 1.18m; // Precio con ITBIS (18%)
                decimal precioTotal = precioConITBIS * cantidades[i];
                totalGeneral += precioTotal;

                table.AddCell(new PdfPCell(new Phrase(descripciones[i], regularFont)));
                table.AddCell(new PdfPCell(new Phrase(cantidades[i].ToString(), regularFont)));
                table.AddCell(new PdfPCell(new Phrase(precioConITBIS.ToString("C"), regularFont)));
                table.AddCell(new PdfPCell(new Phrase(precioTotal.ToString("C"), regularFont)));
            }

            pdfDoc.Add(table);

            // Resumen de totales
            pdfDoc.Add(new Paragraph(" "));
            pdfDoc.Add(new Paragraph($"SubTotal: {totalGeneral.ToString("C")}", boldFont));
            pdfDoc.Add(new Paragraph($"ITBIS: 0.00", boldFont)); // ITBIS ya calculado
            pdfDoc.Add(new Paragraph($"Total: {totalGeneral.ToString("C")}", boldFont));
            pdfDoc.Add(new Paragraph(" "));

            // Pie de página
            pdfDoc.Add(new Paragraph("SalesMCS v3.0", smallFont));
            pdfDoc.Add(new Paragraph(" "));

            // Cerrar el documento
            pdfDoc.Close();

            // Devolver el PDF generado al navegador
            stream.Position = 0;
            return File(stream, "application/pdf", "Cotizacion.pdf");
        }
    }
}
