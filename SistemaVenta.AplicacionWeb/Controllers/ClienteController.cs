using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVenta.AplicacionWeb.Models.ViewModels;
using SistemaVenta.DAL.DBContext;
using SistemaVenta.Entity;

namespace SistemaVenta.AplicacionWeb.Controllers
{
    public class ClienteController : Controller
    {
        private readonly DBVENTAContext _context;

        public ClienteController(DBVENTAContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var clientes = await _context.Clientes.ToListAsync(); // Obtener la lista de clientes
            return View(clientes); // Pasar la lista a la vista
        }

        // Acción para manejar el POST del formulario de registro
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(ClienteViewModel clienteViewModel)
        {
            if (ModelState.IsValid) // Validar el modelo
            {
                // Crear un nuevo cliente a partir del ViewModel
                var cliente = new Cliente
                {
                    Nombre = clienteViewModel.Nombre,
                    Apellidos = clienteViewModel.Apellidos,
                    TipoIdentificacion = clienteViewModel.TipoIdentificacion,
                    NumeroIdentificacion = clienteViewModel.NumeroIdentificacion,
                    NumeroTelefono = clienteViewModel.NumeroTelefono,
                    NumeroCelular = clienteViewModel.NumeroCelular,
                    Correo = clienteViewModel.Correo,
                    Direccion = clienteViewModel.Direccion
                };

                // Agregar cliente a la base de datos
                _context.Add(cliente);
                await _context.SaveChangesAsync(); // Guardar cambios
                return RedirectToAction(nameof(Index)); // Redirigir a la acción Index
            }

            // Si el modelo no es válido, devolver la misma vista con los errores
            var clientes = await _context.Clientes.ToListAsync();
            return View("Index", clientes);
        }
    }
}
