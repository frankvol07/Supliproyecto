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
                return RedirectToAction("NuevaVenta");
            }

            // Si hay errores, vuelve a mostrar el formulario
            return View(model);
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
        public IActionResult GenerarFactura([FromBody] FacturaViewModels factura)
        {
            using (var memoryStream = new MemoryStream())
            {
                // Tamaño de 80mm de ancho (227 puntos) y longitud ajustable (por ejemplo, 350 puntos)
                var pageSize = new Rectangle(227, 400);
                var document = new Document(pageSize, 10, 10, 10, 10);
                var writer = PdfWriter.GetInstance(document, memoryStream);
                document.Open();

                // Encabezado de la factura
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
                var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);

                document.Add(new Paragraph("========Factura========", titleFont) { Alignment = Element.ALIGN_CENTER });
                document.Add(new Paragraph("\n"));

                // Detalles del cliente
                document.Add(new Paragraph($"Cliente: {factura.nombreCliente}", normalFont));
                document.Add(new Paragraph($"Documento: {factura.documentoCliente}", normalFont));
                document.Add(new Paragraph($"ID Cliente: {factura.clienteId}", normalFont));
                document.Add(new Paragraph("\n"));

                // Lista de productos
                document.Add(new Paragraph("Productos:", normalFont));
                foreach (var producto in factura.productos)
                {
                    document.Add(new Paragraph($"- Producto: {producto.nombreProducto}", normalFont));
                    document.Add(new Paragraph($"  Cantidad: {producto.cantidad}", normalFont));
                    document.Add(new Paragraph($"  Precio: {producto.precio}", normalFont));
                    document.Add(new Paragraph($"  Total: {producto.total}\n", normalFont));
                }

                document.Add(new Paragraph("==================================", normalFont));
                document.Add(new Paragraph($"Sub Total: {factura.subTotal}", normalFont));
                document.Add(new Paragraph($"IGV: {factura.igv}", normalFont));
                document.Add(new Paragraph($"Total: {factura.total}", normalFont));
                document.Add(new Paragraph("\n"));

                // Mensaje de agradecimiento
                document.Add(new Paragraph("===Gracias por su compra===", titleFont) { Alignment = Element.ALIGN_CENTER });

                document.Close();
                writer.Close();

                // Retornar el PDF
                Response.Headers["Content-Disposition"] = "inline; filename=Factura.pdf";
                Response.ContentType = "application/pdf";
                return File(memoryStream.ToArray(), "application/pdf");
            }
        }

    }
}
