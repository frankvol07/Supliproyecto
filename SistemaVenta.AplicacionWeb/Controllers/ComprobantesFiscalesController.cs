using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVenta.AplicacionWeb.Models.ViewModels;
using SistemaVenta.DAL.DBContext;
using SistemaVenta.Entity;

namespace SistemaVenta.AplicacionWeb.Controllers
{
    public class ComprobantesFiscalesController : Controller
    {
        private readonly DBVENTAContext _context;

        public ComprobantesFiscalesController(DBVENTAContext context)
        {
            _context = context;
        }

        // Bloque 1: Vista para Consultar Comprobantes Fiscales
        public async Task<IActionResult> Consultar()
        {
            var comprobantes = await _context.ComprobantesFiscales.ToListAsync();
            var configRNC = await _context.ConfigRNCs.FirstOrDefaultAsync();

            ViewBag.ConfigRNCValor = configRNC?.Valor ?? false; // Si no existe, se asume que está desactivado

            return View(comprobantes);
        }

        // Bloque 2: Vista para Crear Comprobante Fiscal
        public IActionResult Crear()
        {
            return View();
        }

        // POST: /ComprobantesFiscales/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(ComprobanteFiscalViewModel model)
        {
            if (ModelState.IsValid)
            {
                var nuevoComprobante = new ComprobanteFiscal
                {
                    Tipo = model.Tipo,
                    NCF_Desde = model.NCF_Desde,
                    NCF_Hasta = model.NCF_Hasta,
                    NCF_Actual = model.NCF_Desde, // Inicializar NCF_Actual con el valor de NCF_Desde
                    NCF_Restan = int.Parse(model.NCF_Hasta) - int.Parse(model.NCF_Desde) + 1, // Calcular los restantes
                    Fecha_Vto = model.Fecha_Vto
                };

                _context.ComprobantesFiscales.Add(nuevoComprobante);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Consultar));
            }

            return View(model);
        }

        // Eliminar Comprobante Fiscal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var comprobante = await _context.ComprobantesFiscales.FindAsync(id);
            if (comprobante != null)
            {
                _context.ComprobantesFiscales.Remove(comprobante);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Consultar));
        }
        // Activar venta con RNC
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivarRNC()
        {
            var configRNC = await _context.ConfigRNCs.FirstOrDefaultAsync(); // Obtener la única configuración
            if (configRNC != null)
            {
                configRNC.Valor = true; // Activar la opción
                await _context.SaveChangesAsync(); // Guardar cambios en la base de datos
            }
            return RedirectToAction(nameof(Consultar)); // Redirigir a la vista Consultar
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DesactivarRNC()
        {
            var configRNC = await _context.ConfigRNCs.FirstOrDefaultAsync(); // Obtener la única configuración
            if (configRNC != null)
            {
                configRNC.Valor = false; // Desactivar la opción
                await _context.SaveChangesAsync(); // Guardar cambios en la base de datos
            }
            return RedirectToAction(nameof(Consultar)); // Redirigir a la vista Consultar
        }

    }
}
