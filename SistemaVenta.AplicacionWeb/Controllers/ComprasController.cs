using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVenta.AplicacionWeb.Models.ViewModels;
using SistemaVenta.DAL.DBContext;
using SistemaVenta.Entity;
using System;

namespace SistemaVenta.AplicacionWeb.Controllers
{
    public class ComprasController : Controller
    {
        private readonly DBVENTAContext _context;

        public ComprasController(DBVENTAContext context)
        {
            _context = context;
        }

        // GET: Compras/Create
        public async Task<IActionResult> Index()
        {
            // Obtener la lista de proveedores (IdProveedor y Nombre)
            ViewBag.Proveedores = await _context.Proveedores
                .Select(p => new {  p.Nombre })
                .ToListAsync();

            // Obtener la lista de productos (IdProducto y Descripción)
            ViewBag.Productos = await _context.Productos
                .Where(p => p.EsActivo == true) // Solo productos activos
                .Select(p => new {  p.Descripcion })
                .ToListAsync();

            return View();
        }

        // POST: Compras/Create
        [HttpPost]
        public async Task<IActionResult> GenerarFactura([FromBody] CompraViewModel model)
        {
            if (model == null || !ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Datos inválidos para generar la factura." });
            }

            // Crear el registro en la base de datos
            var compra = new Compra
            {
                NombreProveedor = model.NombreProveedor,
                NombreProducto = model.NombreProducto,
                Costo = model.Costo,
                Cantidad = model.Cantidad,
                Precio = model.Precio,
                Descripcion = model.Descripcion,
                FormaPago = model.FormaPago,
                NoTarjeta = model.NoTarjeta,
                NoCuenta = model.NoCuenta,
                MontoTotal = model.MontoTotal,
                MontoRestante = model.MontoRestante,
                NoComprobanteFiscal = model.NoComprobanteFiscal
            };

            _context.Compras.Add(compra);
            await _context.SaveChangesAsync();

            // Buscar el producto por NombreProducto
            var producto = await _context.Productos
                .FirstOrDefaultAsync(p => p.Descripcion == model.NombreProducto);

            if (producto == null)
            {
                return BadRequest(new { success = false, message = "El producto especificado no existe." });
            }

            // Actualizar el stock sumando la cantidad
            producto.Stock += model.Cantidad;

            // Guardar los cambios en la base de datos
            _context.Productos.Update(producto);
            await _context.SaveChangesAsync();

            var negocio = _context.Negocios.FirstOrDefault();
            if (negocio == null)
            {
                return BadRequest("No se encontró información del negocio.");
            }

            using (var memoryStream = new MemoryStream())
            {
                var pageSize = new Rectangle(227, 700);
                var document = new Document(pageSize, 10, 10, 10, 10);
                var writer = PdfWriter.GetInstance(document, memoryStream);
                document.Open();

                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 8);
                var tableHeaderFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8);

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

                PdfPTable table = new PdfPTable(4); // 4 columnas
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 3f, 2f, 2f, 3f }); // Configurar ancho relativo de columnas

                // Crear estilo base para celdas (sin bordes)
                var cellStyle = new PdfPCell
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    PaddingTop = 3,
                    PaddingBottom = 3
                };

                // Encabezados de la tabla
                var headers = new string[] { "Producto", "Cantidad", "Costo", "Total" };
                foreach (var header in headers)
                {
                    var cell = new PdfPCell(new Phrase(header, tableHeaderFont)) // Fuente más pequeña
                    {
                        Border = Rectangle.NO_BORDER,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    };
                    table.AddCell(cell);
                }

                // Contenido de la tabla
                table.AddCell(new PdfPCell(new Phrase(model.NombreProducto ?? "", normalFont)) { Border = Rectangle.NO_BORDER });
                table.AddCell(new PdfPCell(new Phrase(model.Cantidad.ToString(), normalFont)) { Border = Rectangle.NO_BORDER });
                table.AddCell(new PdfPCell(new Phrase(model.Costo.ToString("C"), normalFont)) { Border = Rectangle.NO_BORDER });
                table.AddCell(new PdfPCell(new Phrase(model.MontoTotal.ToString("C"), normalFont)) { Border = Rectangle.NO_BORDER });

                // Añadir la tabla al documento
                document.Add(table);

                // Información adicional
                document.Add(new Paragraph("\n", normalFont));
                document.Add(new Paragraph($"Proveedor: {model.NombreProveedor}", normalFont));
                document.Add(new Paragraph($"Forma de Pago: {model.FormaPago}", normalFont));
                document.Add(new Paragraph($"NCF: {model.NoComprobanteFiscal}", normalFont));
                document.Add(new Paragraph("\nGracias por su compra.", normalFont) { Alignment = Element.ALIGN_CENTER });

                document.Close();

                // Retornar el PDF como un archivo descargable
                return File(memoryStream.ToArray(), "application/pdf", "Factura.pdf");
            }
        }



    }
}
