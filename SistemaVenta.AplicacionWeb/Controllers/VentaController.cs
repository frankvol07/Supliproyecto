using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using SistemaVenta.AplicacionWeb.Models.ViewModels;
using SistemaVenta.AplicacionWeb.Utilidades.Response;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.Entity;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using SistemaVenta.DAL.DBContext; // Agregado para el contexto de base de datos
using Microsoft.EntityFrameworkCore;
using iTextSharp.text.pdf;
using iTextSharp.text;

namespace SistemaVenta.AplicacionWeb.Controllers
{
    [Authorize]
    public class VentaController : Controller
    {
        private readonly ITipoDocumentoVentaService _tipoDocumentoVentaServicio;
        private readonly IVentaService _ventaServicio;
        private readonly IMapper _mapper;
        private readonly IConverter _converter;
        private readonly DBVENTAContext _context; // Agregado para acceder al contexto de la base de datos

        public VentaController(ITipoDocumentoVentaService tipoDocumentoVentaServicio,
            IVentaService ventaServicio,
            IMapper mapper,
            IConverter converter,
            DBVENTAContext context // Agregado para el contexto de base de datos
            )
        {
            _tipoDocumentoVentaServicio = tipoDocumentoVentaServicio;
            _ventaServicio = ventaServicio;
            _mapper = mapper;
            _converter = converter;
            _context = context; // Inicializado el contexto de base de datos
        }

        public async Task<IActionResult> NuevaVenta()
        {
            var fechaActual = DateTime.Now.Date;
            var cajaAbierta = await _context.Cajas
                .FirstOrDefaultAsync(c => c.FechaApertura.Date == fechaActual && c.Estado == true);

            if (cajaAbierta == null)
            {
                TempData["MensajeError"] = "La caja está cerrada. No se pueden realizar ventas.";
                return RedirectToAction("MostrarMensajeCajaCerrada");
            }

            // Verificar si ConfigRNC está activo
            var configRNC = await _context.ConfigRNCs.FirstOrDefaultAsync(c => c.Nombre == "ActivarNCF");

            if (configRNC != null && configRNC.Valor)
            {
                // Obtener tipos de comprobantes fiscales disponibles
                var comprobantes = await _context.ComprobantesFiscales
                    .Where(c => c.Fecha_Vto >= fechaActual && c.NCF_Restan > 0)
                    .Select(c => new { c.Id, c.Tipo })
                    .ToListAsync();

                ViewBag.Comprobantes = comprobantes;
            }
            else
            {
                ViewBag.Comprobantes = null;
            }

            // Obtener la lista de clientes
            var clientes = await _context.Clientes.ToListAsync();
            if (clientes == null || !clientes.Any())
            {
                TempData["MensajeError"] = "No se encontraron clientes disponibles.";
                return RedirectToAction("Index");
            }

            ViewBag.Clientes = clientes;
            ViewBag.EsCajaCerrada = false;

            return View();
        }



        public IActionResult MostrarMensajeCajaCerrada()
        {
            return View("NuevaVenta");
        }


        public IActionResult HistorialVenta()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ListaTipoDocumentoVenta()
        {
            List<VMTipoDocumentoVenta> vmListaTipoDocumentos = _mapper.Map<List<VMTipoDocumentoVenta>>(await _tipoDocumentoVentaServicio.Lista());

            return StatusCode(StatusCodes.Status200OK, vmListaTipoDocumentos);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerProductos(string busqueda)
        {
            List<VMProducto> vmListaProductos = _mapper.Map<List<VMProducto>>(await _ventaServicio.ObtenerProductos(busqueda));
            return StatusCode(StatusCodes.Status200OK, vmListaProductos);
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarVenta([FromBody] VMVenta modelo)
        {
            // Verificar si la caja está cerrada antes de proceder
            var fechaActual = DateTime.Now.Date;
            var cajaAbierta = await _context.Cajas
                .FirstOrDefaultAsync(c => c.FechaApertura.Date == fechaActual && c.Estado == true);

            if (cajaAbierta == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "La caja está cerrada. No se pueden realizar ventas.");
            }

            GenericResponse<VMVenta> gResponse = new GenericResponse<VMVenta>();

            try
            {
                ClaimsPrincipal claimUser = HttpContext.User;
                string idUsuario = claimUser.Claims
                    .Where(c => c.Type == ClaimTypes.NameIdentifier)
                    .Select(c => c.Value).SingleOrDefault();

                modelo.IdUsuario = int.Parse(idUsuario);

                Venta venta_creada = await _ventaServicio.Registrar(_mapper.Map<Venta>(modelo));
                modelo = _mapper.Map<VMVenta>(venta_creada);

                gResponse.Estado = true;
                gResponse.Objeto = modelo;
            }
            catch (Exception ex)
            {
                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;
            }
            return StatusCode(StatusCodes.Status200OK, gResponse);
        }



        [HttpPost]
        public IActionResult CrearVenta(CxCobrarViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Crear una nueva factura
                var factura = new Factura
                {
                    ClienteID = model.ClienteId,
                    FechaEmision = DateTime.Now, // Fecha actual
                    FechaVencimiento = DateTime.Now.AddDays(30), // Fecha + 30 días
                    MontoTotal = model.MontoTotal,
                    Estado = "Pendiente" // Estado por defecto
                };

                _context.Facturas.Add(factura);
                _context.SaveChanges(); // Guarda la factura para obtener el ID

                // Crear un nuevo pago
                var pago = new Pago
                {
                    FacturaID = factura.FacturaID, // ID de la factura recién creada
                    MontoPagado = 0, // Monto pagado inicial
                    FechaPago = DateTime.Now, // Fecha actual
                    Recargo = null // Recargo opcional, inicializado en null
                };

                _context.Pagos.Add(pago);
                _context.SaveChanges(); // Guarda el pago

                // Redirige a la misma vista después del proceso
                return NoContent(); // O Json(new { success = true });
            }

            // Si hay errores, retorna un estado de error
            return BadRequest(ModelState);
        }


            public async Task<IActionResult> Historial(string numeroVenta, string fechaInicio, string fechaFin)
        {
            List<VMVenta> vmHistorialVenta = _mapper.Map<List<VMVenta>>(await _ventaServicio.Historial(numeroVenta, fechaInicio, fechaFin));
            return StatusCode(StatusCodes.Status200OK, vmHistorialVenta);
        }

        public IActionResult MostrarPDFVenta(string numeroVenta)
        {
            string urlPlantillaVista = $"{this.Request.Scheme}://{this.Request.Host}/Plantilla/PDFVenta?numeroVenta={numeroVenta}";

            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = new GlobalSettings()
                {
                    PaperSize = PaperKind.A4,
                    Orientation = Orientation.Portrait,
                },
                Objects =
                {
                    new ObjectSettings()
                    {
                        Page = urlPlantillaVista
                    }
                }
            };
            var archivoPDF = _converter.Convert(pdf);
            return File(archivoPDF, "application/pdf");
        }


        [HttpPost]
        [Authorize]
        public IActionResult GenerarFactura([FromBody] FacturaViewModels factura, string tipoComprobante)
        {
            // Verificar si la funcionalidad de NCF está activada
            var configRNC = _context.ConfigRNCs.FirstOrDefault();
            bool isRNCActive = configRNC != null && configRNC.Valor;

            string ncfActual = null;
            string tipoNCF = null;
            DateTime? fechaVencimiento = null;

            if (isRNCActive)
            {
                var comprobante = _context.ComprobantesFiscales
                    .Where(c => c.Tipo == tipoComprobante && c.NCF_Restan > 0 && c.Fecha_Vto > DateTime.Now)
                    .OrderBy(c => c.NCF_Actual)
                    .FirstOrDefault();

                if (comprobante == null)
                {
                    return BadRequest("No hay comprobantes fiscales disponibles para el tipo seleccionado o han vencido.");
                }

                ncfActual = comprobante.NCF_Actual;
                tipoNCF = comprobante.Tipo;
                fechaVencimiento = comprobante.Fecha_Vto;
                var nuevoNCF = IncrementarNCF(ncfActual);

                if (string.Compare(nuevoNCF, comprobante.NCF_Hasta) > 0)
                {
                    return BadRequest("Se ha alcanzado el límite de la serie de NCF.");
                }

                comprobante.NCF_Actual = nuevoNCF;
                comprobante.NCF_Restan -= 1;
                _context.ComprobantesFiscales.Update(comprobante);
                _context.SaveChanges();
                var nuevaVentaRNC = new VentaRNC
                {
                    RncCedula = factura.documentoCliente ?? "Sin Identificar", // RNC o cédula del cliente
                    TipoId = "Identificacion", // Asume que es un RNC. Ajusta según el caso.
                    BienServicioComprado = string.Join(", ", factura.productos.Select(p => p.nombreProducto)), // Lista de productos
                    Ncf = tipoNCF + ncfActual, // NCF generado
                    FechaComprobante = DateTime.Now, // Fecha actual
                    FechaPago = DateTime.Now, // Fecha de pago (puedes cambiar según tu lógica)
                    TotalMontoFacturado = decimal.TryParse(factura.total, out var total) ? total : 0, // Conversión segura
                    ItbisFacturado = decimal.TryParse(factura.igv, out var itbis) ? itbis : 0 // Conversión segura // ITBIS facturado
                };

                _context.VentaRNC.Add(nuevaVentaRNC);
                _context.SaveChanges();
            }

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

                document.Add(new Paragraph("Factura:", normalFont));

                if (isRNCActive && ncfActual != null)
                {
                    document.Add(new Paragraph($"NCF: {tipoNCF}{ncfActual}", normalFont));
                    document.Add(new Paragraph($"Fecha de Vencimiento: {fechaVencimiento:dd/MM/yyyy}", normalFont));
                }

                document.Add(new Paragraph($"Fecha: {DateTime.Now:dd/MM/yyyy}", normalFont));
                document.Add(new Paragraph($"CLIENTE: {factura.documentoCliente ?? "Sin Identificar"}", normalFont));
                document.Add(new Paragraph($"Nombre Cliente: {factura.nombreCliente ?? "Cliente Genérico"}", normalFont));

                document.Add(new Paragraph("\n"));

                PdfPTable table = new PdfPTable(4);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 1, 3, 1, 1 });

                table.AddCell(new PdfPCell(new Phrase("Cant", normalFont)) { Border = 0 });
                table.AddCell(new PdfPCell(new Phrase("Descripcion", normalFont)) { Border = 0 });
                table.AddCell(new PdfPCell(new Phrase("Precio", normalFont)) { Border = 0 });
                table.AddCell(new PdfPCell(new Phrase("Valor", normalFont)) { Border = 0 });

                foreach (var producto in factura.productos)
                {
                    table.AddCell(new PdfPCell(new Phrase(producto.cantidad.ToString(), normalFont)) { Border = 0 });
                    table.AddCell(new PdfPCell(new Phrase(producto.nombreProducto, normalFont)) { Border = 0 });
                    table.AddCell(new PdfPCell(new Phrase($"{producto.precio:C}", normalFont)) { Border = 0 });
                    table.AddCell(new PdfPCell(new Phrase($"{producto.total:C}", normalFont)) { Border = 0 });
                }

                document.Add(table);
                document.Add(new Paragraph("\n"));
                document.Add(new Paragraph($"SubTotal: {factura.subTotal:C}", normalFont));
                document.Add(new Paragraph($"ITBIS: {factura.igv:C}", normalFont));
                document.Add(new Paragraph($"Total: {factura.total:C}", normalFont));
                document.Add(new Paragraph($"Monto Restante: {factura.montoRestante:C}", normalFont));
                document.Add(new Paragraph($"Método de Pago: {factura.metodoPago ?? "No especificado"}", normalFont));

                document.Add(new Paragraph("\n"));

                document.Add(new Paragraph("GRACIAS POR PREFERIRNOS!!!", normalFont) { Alignment = Element.ALIGN_CENTER });

                document.Close();
                writer.Close();

                Response.Headers["Content-Disposition"] = "inline; filename=Factura.pdf";
                Response.ContentType = "application/pdf";
                return File(memoryStream.ToArray(), "application/pdf");
            }

        }

        private string IncrementarNCF(string ncfActual)
        {
            // Convertimos el NCF actual en un número entero
            var numeroActual = int.Parse(ncfActual);

            // Incrementamos el número
            var numeroIncrementado = numeroActual + 1;

            // Convertimos el número incrementado de nuevo a string y aseguramos que tenga 8 caracteres
            var nuevoNCF = numeroIncrementado.ToString().PadLeft(8, '0');

            return nuevoNCF;
        }
    }

    }








