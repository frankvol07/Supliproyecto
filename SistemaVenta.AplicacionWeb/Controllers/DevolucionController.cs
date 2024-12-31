using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVenta.AplicacionWeb.Models;
using SistemaVenta.DAL.DBContext;
using SistemaVenta.Entity;
using System;
using System.Linq;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;

namespace SistemaVenta.AplicacionWeb.Controllers
{
    public class DevolucionController : Controller
    {
        private readonly DBVENTAContext _context;

        public DevolucionController(DBVENTAContext context)
        {
            _context = context;
        }

        // Acción para mostrar la vista principal de devoluciones
        public IActionResult Index()
        {
            return View();
        }

        // Acción para buscar las ventas dentro del rango de fechas
        [HttpPost]
        public async Task<IActionResult> BuscarVentas(DateTime fechaInicio, DateTime fechaFin)
        {
            var ventasDetalles = await _context.Venta
                .Where(v => v.FechaRegistro >= fechaInicio && v.FechaRegistro <= fechaFin)
                .SelectMany(v => v.DetalleVenta, (v, d) => new
                {
                    v.IdVenta,
                    v.NumeroVenta,
                    v.NombreCliente,
                    TotalVenta = v.Total,
                    d.DescripcionProducto,
                    d.MarcaProducto,
                    d.Cantidad,
                    d.Precio,
                    TotalProducto = d.Total,
                    v.FechaRegistro
                })
                .ToListAsync();

            return Json(ventasDetalles);
        }

        // Acción para generar PDF y luego proceder con la devolución
        public async Task<IActionResult> GenerarDevolucionPDF(int idVenta)
        {
            // Obtener la venta y sus detalles
            var venta = await _context.Venta
                .Include(v => v.DetalleVenta)
                .FirstOrDefaultAsync(v => v.IdVenta == idVenta);

            if (venta == null)
            {
                return NotFound("No se encontró la venta especificada.");
            }

            var negocio = await _context.Negocios.FirstOrDefaultAsync();
            if (negocio == null)
            {
                return BadRequest("No se encontró información del negocio.");
            }

            // Generar el PDF de la devolución
            using (var memoryStream = new MemoryStream())
            {
                var pageSize = new Rectangle(227, 700);
                var document = new Document(pageSize, 10, 10, 10, 10);
                var writer = PdfWriter.GetInstance(document, memoryStream);
                document.Open();

                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 8);

                if (!string.IsNullOrEmpty(negocio.UrlLogo))
                {
                    var logoImage = Image.GetInstance(negocio.UrlLogo);
                    logoImage.ScaleToFit(50f, 50f);
                    logoImage.Alignment = Image.ALIGN_CENTER;
                    document.Add(logoImage);
                }

                document.Add(new Paragraph(negocio.Nombre ?? "Nombre del Negocio", titleFont) { Alignment = Element.ALIGN_CENTER });
                document.Add(new Paragraph($"RNC: {negocio.NumeroDocumento}", normalFont) { Alignment = Element.ALIGN_CENTER });
                document.Add(new Paragraph($"TEL: {negocio.Telefono}", normalFont) { Alignment = Element.ALIGN_CENTER });
                document.Add(new Paragraph($"{negocio.Direccion}", normalFont) { Alignment = Element.ALIGN_CENTER });
                document.Add(new Paragraph("\n"));

                document.Add(new Paragraph("Devolución de Venta", normalFont));
                document.Add(new Paragraph($"Número de Venta: {venta.NumeroVenta}", normalFont));
                document.Add(new Paragraph($"Fecha de Venta: {venta.FechaRegistro:dd/MM/yyyy}", normalFont));
                document.Add(new Paragraph($"CLIENTE: {venta.NombreCliente ?? "Sin Identificar"}", normalFont));
                document.Add(new Paragraph($"Total de Venta: {venta.Total:C}", normalFont));
                document.Add(new Paragraph("\n"));

                PdfPTable table = new PdfPTable(4);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 1, 3, 1, 1 });

                table.AddCell(new PdfPCell(new Phrase("Cant", normalFont)) { Border = 0 });
                table.AddCell(new PdfPCell(new Phrase("Producto", normalFont)) { Border = 0 });
                table.AddCell(new PdfPCell(new Phrase("Precio", normalFont)) { Border = 0 });
                table.AddCell(new PdfPCell(new Phrase("Total", normalFont)) { Border = 0 });

                foreach (var detalle in venta.DetalleVenta)
                {
                    table.AddCell(new PdfPCell(new Phrase(detalle.Cantidad.ToString(), normalFont)) { Border = 0 });
                    table.AddCell(new PdfPCell(new Phrase(detalle.DescripcionProducto, normalFont)) { Border = 0 });
                    table.AddCell(new PdfPCell(new Phrase($"{detalle.Precio:C}", normalFont)) { Border = 0 });
                    table.AddCell(new PdfPCell(new Phrase($"{detalle.Total:C}", normalFont)) { Border = 0 });

                    // Actualizar el stock de los productos devueltos
                    var producto = await _context.Productos
                        .FirstOrDefaultAsync(p => p.Descripcion == detalle.DescripcionProducto);

                    if (producto != null)
                    {
                        producto.Stock += detalle.Cantidad; // Sumar la cantidad devuelta al stock
                        _context.Productos.Update(producto);
                    }
                }

                document.Add(table);
                document.Add(new Paragraph("\n"));
                document.Add(new Paragraph("GRACIAS POR PREFERIRNOS!!!", normalFont) { Alignment = Element.ALIGN_CENTER });

                document.Close();
                writer.Close();

                // Eliminar la venta y sus detalles
                _context.DetalleVenta.RemoveRange(venta.DetalleVenta);
                _context.Venta.Remove(venta);
                await _context.SaveChangesAsync();

                // Devolver el archivo PDF generado
                Response.Headers["Content-Disposition"] = $"inline; filename=Devolucion_{venta.NumeroVenta}.pdf";
                Response.ContentType = "application/pdf";
                return File(memoryStream.ToArray(), "application/pdf");
            }
        }
    }
}
