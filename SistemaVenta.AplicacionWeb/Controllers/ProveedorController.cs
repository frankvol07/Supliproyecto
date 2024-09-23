using Microsoft.AspNetCore.Mvc;
using SistemaVenta.Entity;
using SistemaVenta.AplicacionWeb.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using SistemaVenta.DAL.DBContext;

namespace SistemaVenta.AplicacionWeb.Controllers
{
    public class ProveedorController : Controller
    {
        private readonly DBVENTAContext _context;

        public ProveedorController(DBVENTAContext context)
        {
            _context = context;
        }

        // Mostrar la vista con la lista de proveedores
        public async Task<IActionResult> Index()
        {
            var proveedores = _context.Proveedores.ToList(); // Obtener los datos de la base de datos
            var proveedorViewModel = new ProveedorViewModel
            {
                ProveedoresList = proveedores.Select(p => new ProveedorViewModel
                {
                    IdProveedor = p.IdProveedor,
                    Identificacion = p.Identificacion,
                    Nombre = p.Nombre,
                    Correo = p.Correo,
                    Telefono = p.Telefono,
                    Direccion = p.Direccion
                }).ToList()
            };

            return View(proveedorViewModel);
        }

        // Agregar o Editar un proveedor
        [HttpPost]
        public async Task<IActionResult> Guardar(ProveedorViewModel proveedorViewModel)
        {
            if (ModelState.IsValid)
            {
                if (proveedorViewModel.IdProveedor == 0) // Nuevo proveedor
                {
                    var nuevoProveedor = new Proveedor
                    {
                        Identificacion = proveedorViewModel.Identificacion,
                        Nombre = proveedorViewModel.Nombre,
                        Correo = proveedorViewModel.Correo,
                        Telefono = proveedorViewModel.Telefono,
                        Direccion = proveedorViewModel.Direccion
                    };

                    _context.Proveedores.Add(nuevoProveedor);
                }
                else // Editar proveedor existente
                {
                    var proveedorExistente = _context.Proveedores.Find(proveedorViewModel.IdProveedor);
                    if (proveedorExistente != null)
                    {
                        proveedorExistente.Identificacion = proveedorViewModel.Identificacion;
                        proveedorExistente.Nombre = proveedorViewModel.Nombre;
                        proveedorExistente.Correo = proveedorViewModel.Correo;
                        proveedorExistente.Telefono = proveedorViewModel.Telefono;
                        proveedorExistente.Direccion = proveedorViewModel.Direccion;

                        _context.Proveedores.Update(proveedorExistente);
                    }
                }

                await _context.SaveChangesAsync(); // Guardar cambios en la base de datos
                return RedirectToAction(nameof(Index)); // Volver al listado
            }

            return View("Index", proveedorViewModel); // Si hay un error, volver a la vista
        }

        // Eliminar un proveedor
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var proveedor = await _context.Proveedores.FindAsync(id);
            if (proveedor != null)
            {
                _context.Proveedores.Remove(proveedor);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
