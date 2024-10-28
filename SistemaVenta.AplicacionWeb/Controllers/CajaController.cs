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
        public async Task<IActionResult> Abrir()
        {
            // Buscar la caja más antigua sin cerrar
            var cajaSinCerrar = await _context.Cajas
                .Where(c => c.FechaCierre == null && c.Estado == true)
                .OrderBy(c => c.FechaApertura)
                .FirstOrDefaultAsync();

            if (cajaSinCerrar != null)
            {
                // Si hay una caja sin cerrar, mostrar un mensaje en la vista
                ViewBag.Message = $"No puedes abrir una nueva caja hasta que cierres la caja abierta el {cajaSinCerrar.FechaApertura.ToString("yyyy-MM-dd")}.";

                // Devolver el ViewModel con la fecha de apertura sin cerrar
                var model = new CajaViewModel
                {
                    CajaId = cajaSinCerrar.CajaId,
                    FechaApertura = cajaSinCerrar.FechaApertura,
                    MontoInicial = cajaSinCerrar.MontoInicial,
                    Estado = cajaSinCerrar.Estado,
                    IsMontoInicialEditable = false // Campo no editable si hay una caja sin cerrar
                };

                return View(model);
            }

            // Si no hay cajas sin cerrar, proceder con la lógica normal para abrir una caja
            ViewBag.Message = null;
            return View(new CajaViewModel
            {
                FechaApertura = DateTime.Now,
                IsMontoInicialEditable = true // Editable al abrir una nueva caja
            });
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
                return View("Abrir", model); // Pasar el modelo de vuelta a la vista
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
                return View("Abrir", model); // Pasar el modelo de vuelta a la vista
            }

            if (model.FechaApertura == default(DateTime))
            {
                ModelState.AddModelError("", "Fecha de apertura no válida.");
                return View("Abrir", model); // Pasar el modelo de vuelta a la vista
            }

            // Verificar que el monto final esté relleno
            if (model.MontoFinal <= 0) // Aquí puedes ajustar la validación según lo que consideres válido
            {
                ModelState.AddModelError("MontoFinal", "El monto final debe ser mayor que cero.");
                return View("Abrir", model); // Pasar el modelo de vuelta a la vista
            }

            // Buscar la caja abierta por fecha de apertura
            var caja = await _context.Cajas.FirstOrDefaultAsync(c => c.FechaApertura.Date == model.FechaApertura.Date && c.Estado == true);

            if (caja == null)
            {
                ModelState.AddModelError("", "No hay caja abierta para la fecha proporcionada.");
                return View("Abrir", model); // Pasar el modelo de vuelta a la vista
            }

            // Actualizar la caja con la fecha de cierre y el monto final
            caja.FechaCierre = DateTime.Now; // Si no se proporciona, usar la fecha y hora actual
            caja.MontoFinal = model.MontoFinal; // Verifica que model.MontoFinal no sea null
            caja.Estado = false; // Marcar como cerrada

            _context.Cajas.Update(caja);
            await _context.SaveChangesAsync();

            ViewBag.Message = "Caja cerrada exitosamente.";
            return RedirectToAction("Abrir"); // Redirigir a Abrir para evitar reenvío del formulario
        }
    }
}
