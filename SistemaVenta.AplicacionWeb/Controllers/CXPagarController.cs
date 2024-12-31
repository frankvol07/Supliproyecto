using AutoMapper;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVenta.AplicacionWeb.Models.ViewModels;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.DAL.DBContext;
using SistemaVenta.Entity;
using System.IO;

namespace SistemaVenta.AplicacionWeb.Controllers
{
    public class CXPagarController : Controller
    {
        private readonly DBVENTAContext _context;

        public CXPagarController(DBVENTAContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Obtener la lista de proveedores para el ViewBag

            return View();
        }
        [HttpGet]
        public async Task<JsonResult> ObtenerComprasCredito()
        {
            var comprasCredito = await _context.Compras
                .Where(c => c.FormaPago == "Credito")
                .Select(c => new CompraViewModel
                {
                    CompraId = c.CompraId,
                    NombreProveedor = c.NombreProveedor,
                    NombreProducto = c.NombreProducto,
                    Costo = c.Costo,
                    Cantidad = c.Cantidad,
                    Precio = c.Precio,
                    Descripcion = c.Descripcion,
                    FormaPago = c.FormaPago,
                    NoTarjeta = c.NoTarjeta,
                    NoCuenta = c.NoCuenta,
                    MontoTotal = c.MontoTotal,
                    MontoRestante = c.MontoRestante,
                    NoComprobanteFiscal = c.NoComprobanteFiscal
                })
                .ToListAsync();

            return Json(comprasCredito);
        }
        [HttpPost]
        public async Task<IActionResult> RealizarAbono(int idCompra, decimal montoAbono)
        {
            var compra = await _context.Compras.FindAsync(idCompra);
            if (compra == null)
            {
                return NotFound("La compra no existe.");
            }

            if (montoAbono > compra.MontoRestante)
            {
                return BadRequest("El monto del abono no puede ser mayor al monto restante.");
            }

            // Actualizar el monto restante
            compra.MontoRestante -= montoAbono;
            if (compra.MontoRestante < 0)
            {
                compra.MontoRestante = 0;
            }

            _context.Compras.Update(compra);
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

                // Espaciado
                document.Add(new iTextSharp.text.Paragraph(" "));


                // Información de la compra y el abono
                var textFont = iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA, 8);
                document.Add(new iTextSharp.text.Paragraph($"Proveedor: {compra.NombreProveedor}", textFont));
                document.Add(new iTextSharp.text.Paragraph($"Producto: {compra.NombreProducto}", textFont));
                document.Add(new iTextSharp.text.Paragraph($"Monto Total: {compra.MontoTotal:C}", textFont));
                document.Add(new iTextSharp.text.Paragraph($"Monto Abonado: {montoAbono:C}", textFont));
                document.Add(new iTextSharp.text.Paragraph($"Monto Restante: {compra.MontoRestante:C}", textFont));
                document.Add(new iTextSharp.text.Paragraph($"Número de Comprobante Fiscal: {compra.NoComprobanteFiscal}", textFont));

                document.Close();
                var pdfBytes = memoryStream.ToArray();

                return File(pdfBytes, "application/pdf", "FacturaAbono.pdf");
            }
        }


    }
}