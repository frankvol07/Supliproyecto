using Microsoft.AspNetCore.Mvc;
using SistemaVenta.AplicacionWeb.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SistemaVenta.DAL.DBContext;
using SistemaVenta.Entity;

namespace SistemaVenta.AplicacionWeb.Controllers
{
    public class CajaController : Controller
    {
        private readonly DBVENTAContext _context;

        public CajaController(DBVENTAContext context)
        {
            _context = context;
        }

        // GET: Caja/Abrir
        public IActionResult Abrir()
        {
            return View(new CajaViewModel());
        }

        // POST: Caja/Abrir
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Abrir(CajaViewModel model, string supervisorNombre)
        {
            // Verificar si el nombre del supervisor es válido
            var supervisor = await _context.Usuarios.FirstOrDefaultAsync(u => u.Nombre == supervisorNombre);
            if (supervisor == null)
            {
                ModelState.AddModelError("", "El nombre del supervisor es incorrecto.");
                return View("Abrir");
            }

            if (ModelState.IsValid)
            {
                var existeCaja = _context.Cajas.Any(c => c.FechaApertura == model.FechaApertura);

                if (existeCaja)
                {
                    ModelState.AddModelError("", "Ya existe una caja abierta para esta fecha.");
                    return View(model);
                }

                // Crear una nueva caja
                var nuevaCaja = new Caja
                {
                    FechaApertura = model.FechaApertura,
                    MontoInicial = model.MontoInicial,
                    Estado = true
                };

                _context.Cajas.Add(nuevaCaja);
                await _context.SaveChangesAsync();
                ViewBag.Message = "Caja abierta exitosamente.";
                return RedirectToAction("NuevaVenta", "Venta");
            }

            return View(model);
        }

        // POST: Caja/Cerrar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cerrar(CajaViewModel model, string supervisorNombre)
        {
            // Verificar si el nombre del supervisor es válido
            var supervisor = await _context.Usuarios.FirstOrDefaultAsync(u => u.Nombre == supervisorNombre);
            if (supervisor == null)
            {
                ModelState.AddModelError("", "El nombre del supervisor es incorrecto.");
                return View("Abrir");
            }

            if (model.FechaApertura == default(DateTime))
            {
                ModelState.AddModelError("", "Fecha de apertura no válida.");
                return View("Abrir");
            }

            // Buscar la caja abierta por fecha de apertura
            var caja = await _context.Cajas.FirstOrDefaultAsync(c => c.FechaApertura.Date == model.FechaApertura.Date && c.Estado == true);

            if (caja == null)
            {
                ModelState.AddModelError("", "No hay caja abierta para la fecha proporcionada.");
                return View("Abrir");
            }

            // Actualizar la caja con la fecha de cierre y el monto final
            caja.FechaCierre = model.FechaCierre ?? System.DateTime.Now;
            caja.MontoFinal = model.MontoFinal;
            caja.Estado = false; // Marcar como cerrada

            _context.Cajas.Update(caja);
            await _context.SaveChangesAsync();

            ViewBag.Message = "Caja cerrada exitosamente.";
            return View("Abrir");
        }
    }
}
