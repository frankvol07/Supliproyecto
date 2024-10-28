using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVenta.AplicacionWeb.Models.ViewModels;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.DAL.DBContext;
using SistemaVenta.Entity;

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
            var proveedores = _context.Proveedores
                .Select(p => new ProveedorViewModel
                {
                    IdProveedor = p.IdProveedor,
                    Nombre = p.Nombre
                }).ToList();

            ViewBag.Proveedores = proveedores;

            // Crear un diccionario para acceder a los nombres de los proveedores
            ViewBag.ProveedoresDict = proveedores.ToDictionary(p => p.IdProveedor, p => p.Nombre);

            var viewModel = new FacturaProveedorViewModel
            {
                Facturas = _context.FacturaProveedores.ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Index(FacturaProveedorViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Procesar los datos y guardarlos en la base de datos
                var factura = new FacturaProveedor
                {
                    ProveedorID = viewModel.ProveedorID,
                    FechaEmision = DateTime.Now, // Fecha actual
                    FechaVencimiento = viewModel.FechaVencimiento,
                    MontoTotal = viewModel.MontoTotal,
                    Detalle = viewModel.Detalle,
                    Estado = viewModel.Estado
                };

                _context.FacturaProveedores.Add(factura);
                await _context.SaveChangesAsync();

                // Redirigir después de guardar los datos para evitar que se repita el POST
                return RedirectToAction(nameof(Index));
            }

            // Si hay errores de validación, se recarga la misma vista con el modelo actual
            var proveedores = await _context.Proveedores
                .ToDictionaryAsync(p => p.IdProveedor, p => p.Nombre);
            ViewBag.ProveedoresDict = proveedores;

            viewModel.Facturas = await _context.FacturaProveedores.ToListAsync();

            return View(viewModel);
        }
        public async Task<IActionResult> MarcarComoPagado(int id)
        {
            var factura = await _context.FacturaProveedores.FindAsync(id);

            if (factura == null)
            {
                return NotFound();
            }

            // Cambiar el estado a "Pagado"
            factura.Estado = "Pagado";

            _context.FacturaProveedores.Update(factura);
            await _context.SaveChangesAsync();

            // Redirigir a la vista principal después de actualizar
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Eliminar(int id)
        {
            var factura = await _context.FacturaProveedores.FindAsync(id);

            if (factura == null)
            {
                return NotFound();
            }

            // Eliminar el registro
            _context.FacturaProveedores.Remove(factura);
            await _context.SaveChangesAsync();

            // Redirigir a la vista principal después de eliminar
            return RedirectToAction(nameof(Index));
        }

    }

}