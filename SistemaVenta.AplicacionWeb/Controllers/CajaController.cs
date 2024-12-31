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
        [HttpPost]
        public async Task<IActionResult> ActualizarDesglose([FromForm] CajaViewModel model)
        {
            try
            {
                // Buscar la última caja activa según la fecha de apertura
                var caja = await _context.Cajas
                    .Where(c => c.Estado == true) // Caja activa
                    .OrderByDescending(c => c.FechaApertura) // Más reciente
                    .FirstOrDefaultAsync();

                if (caja == null)
                {
                    return BadRequest("No se encontró una caja abierta.");
                }

                // Actualizar los valores de billetes y monedas
                caja.Monto2000 = model.Monto2000;
                caja.Monto1000 = model.Monto1000;
                caja.Monto500 = model.Monto500;
                caja.Monto200 = model.Monto200;
                caja.Monto100 = model.Monto100;
                caja.Monto50 = model.Monto50;
                caja.Monto25 = model.Monto25;
                caja.Monto10 = model.Monto10;
                caja.Monto5 = model.Monto5;
                caja.Monto1 = model.Monto1;

                // Calcular el total del desglose
                caja.TotalDesglose = (caja.Monto2000 ?? 0) * 2000 +
                                     (caja.Monto1000 ?? 0) * 1000 +
                                     (caja.Monto500 ?? 0) * 500 +
                                     (caja.Monto200 ?? 0) * 200 +
                                     (caja.Monto100 ?? 0) * 100 +
                                     (caja.Monto50 ?? 0) * 50 +
                                     (caja.Monto25 ?? 0) * 25 +
                                     (caja.Monto10 ?? 0) * 10 +
                                     (caja.Monto5 ?? 0) * 5 +
                                     (caja.Monto1 ?? 0) * 1;

                // Obtener el MontoInicial desde la caja (asegúrate de tener este campo en la base de datos)
                var montoInicial = caja.MontoInicial;

                // Calcular la diferencia y ganancia
                var diferenciaGanancia = caja.TotalDesglose - montoInicial;

                // Si la diferencia es positiva, va a ganancia
                if (diferenciaGanancia > 0)
                {
                    caja.Ganancia = diferenciaGanancia;
                    caja.Diferencia = 0;
                }
                // Si la diferencia es negativa, va a diferencia
                else
                {
                    caja.Ganancia = 0;
                    caja.Diferencia = Math.Abs((decimal)diferenciaGanancia);
                }

                // Guardar los cambios en la base de datos
                _context.Cajas.Update(caja);
                await _context.SaveChangesAsync();

                return Ok("Desglose actualizado correctamente.");
            }
            catch (Exception ex)
            {
                // Loguear el error y devolver un estado de error
                Console.WriteLine($"Error en ActualizarDesglose: {ex.Message}");
                return StatusCode(500, "Error en el servidor.");
            }
        }


    }
}
