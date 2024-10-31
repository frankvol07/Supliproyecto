using Microsoft.AspNetCore.Mvc;

using AutoMapper;
using Newtonsoft.Json;
using SistemaVenta.AplicacionWeb.Models.ViewModels;
using SistemaVenta.AplicacionWeb.Utilidades.Response;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.Entity;
using Microsoft.AspNetCore.Authorization;
using SistemaVenta.DAL.DBContext;

namespace SistemaVenta.AplicacionWeb.Controllers
{
    [Authorize]
    public class ProductoController : Controller
    {
        private readonly DBVENTAContext _context;
        private readonly IMapper _mapper;
       private readonly IProductoService _productoServicio;
        public ProductoController(IMapper mapper, 
            IProductoService productoServicio, DBVENTAContext context)
        {
            _mapper = mapper;
            _productoServicio = productoServicio;
            _context = context;
        }


        public IActionResult Index()
        {
            // Obtener la lista de proveedores
            var productos = _context.Productos
    .Select(p => new VMProducto
    {
        IdProducto = p.IdProducto,
        CodigoBarra = p.CodigoBarra,
        Marca = p.Marca,
        Descripcion = p.Descripcion,
        IdCategoria = p.IdCategoria,
        NombreCategoria = p.IdCategoriaNavigation.Descripcion,
        Stock = p.Stock,
        UrlImagen = p.UrlImagen,
        Precio = p.Precio.HasValue ? p.Precio.Value.ToString("F2") : "0.00", // Convierte a string
        EsActivo = p.EsActivo == true ? 1 : 0 // Convierte a int
    }).ToList();

            return View(productos); 
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Lista()
        {
            List<VMProducto> vmProductoLista = _mapper.Map<List<VMProducto>>(await _productoServicio.Lista());

            return StatusCode(StatusCodes.Status200OK, new { data = vmProductoLista });

            
        }
        [HttpPost]
        public async Task<IActionResult> Crear([FromForm] IFormFile imagen, [FromForm] string modelo)
        {
            GenericResponse<VMProducto> gResponse = new GenericResponse<VMProducto>();

            try
            {
                // Deserializamos el modelo recibido desde la vista
                VMProducto vmProducto = JsonConvert.DeserializeObject<VMProducto>(modelo);

                string nombreImagen = "";
                Stream imagenStream = null;

                // Si hay imagen, generamos el nombre y obtenemos el stream
                if (imagen != null)
                {
                    string nombre_en_codigo = Guid.NewGuid().ToString("N");
                    string extension = Path.GetExtension(imagen.FileName);
                    nombreImagen = string.Concat(nombre_en_codigo, extension);
                    imagenStream = imagen.OpenReadStream();
                }

                // Crear el producto, el Código de Barras se genera automáticamente en el servicio
                Producto producto_creado = await _productoServicio.Crear(_mapper.Map<Producto>(vmProducto), imagenStream, nombreImagen);

                // Mapear el producto creado para la respuesta
                vmProducto = _mapper.Map<VMProducto>(producto_creado);

                gResponse.Estado = true;
                gResponse.Objeto = vmProducto;

            }
            catch (Exception ex)
            {
                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;
            }

            // Devolver la respuesta con el estado
            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpPut]
        public async Task<IActionResult> Editar([FromForm] IFormFile imagen, [FromForm] string modelo)
        {
            GenericResponse<VMProducto> gResponse = new GenericResponse<VMProducto>();

            try
            {
                // Deserializamos el modelo recibido desde la vista
                VMProducto vmProducto = JsonConvert.DeserializeObject<VMProducto>(modelo);

                Stream imagenStream = null;

                // Si hay imagen, obtenemos el stream
                if (imagen != null)
                {
                    imagenStream = imagen.OpenReadStream();
                }

                // Editar el producto, el Código de Barras no se modifica en el servicio
                Producto producto_editado = await _productoServicio.Editar(_mapper.Map<Producto>(vmProducto), imagenStream);

                // Mapear el producto editado para la respuesta
                vmProducto = _mapper.Map<VMProducto>(producto_editado);

                gResponse.Estado = true;
                gResponse.Objeto = vmProducto;

            }
            catch (Exception ex)
            {
                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;
            }

            // Devolver la respuesta con el estado
            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpDelete]
        public async Task <IActionResult> Eliminar(int IdProducto)
        {
            GenericResponse<string> gResponse = new GenericResponse<string>();

            try
            {
                gResponse.Estado = await _productoServicio.Eliminar(IdProducto);

            }
            catch (Exception ex)
            {
                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;
            }
            return StatusCode(StatusCodes.Status200OK, gResponse);


        }


        [HttpPost]
        public IActionResult Guardar(VMProducto vmProducto)
        {
            if (ModelState.IsValid)
            {
                Producto producto;

                if (vmProducto.IdProducto == 0) // Si IdProducto es 0, es un producto nuevo
                {
                    producto = new Producto
                    {
                        CodigoBarra = vmProducto.CodigoBarra,
                        Marca = vmProducto.Marca,
                        Descripcion = vmProducto.Descripcion,
                        Stock = vmProducto.Stock,
                        Precio = decimal.Parse(vmProducto.Precio),
                        EsActivo = vmProducto.EsActivo == 1
                    };
                    _context.Productos.Add(producto);
                }
                else // Si IdProducto es diferente de 0, es una actualización
                {
                    producto = _context.Productos.Find(vmProducto.IdProducto);
                    if (producto == null)
                        return NotFound();

                    producto.CodigoBarra = vmProducto.CodigoBarra;
                    producto.Marca = vmProducto.Marca;
                    producto.Descripcion = vmProducto.Descripcion;
                    producto.Stock = vmProducto.Stock;
                    producto.Precio = decimal.Parse(vmProducto.Precio);
                    producto.EsActivo = vmProducto.EsActivo == 1;

                    _context.Productos.Update(producto);
                }

                _context.SaveChanges();
                return RedirectToAction("Index"); // Redirigir a la lista de productos
            }

            // En caso de que el modelo no sea válido, volver a la lista de productos
            return RedirectToAction("Index");
        }


        // GET: /Producto/Eliminar/{id}
        public async Task<IActionResult> Eliminar2(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto != null)
            {
                _context.Productos.Remove(producto);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: /Producto/GetProducto/{id}
        [HttpGet]
        public async Task<IActionResult> GetProducto(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null) return NotFound();

            var vmProducto = new VMProducto
            {
                IdProducto = producto.IdProducto,
                CodigoBarra = producto.CodigoBarra,
                Marca = producto.Marca,
                Descripcion = producto.Descripcion,
                Stock = producto.Stock,
                Precio = producto.Precio.HasValue ? producto.Precio.Value.ToString("F2") : "0.00", // Convierte a string
                EsActivo = producto.EsActivo == true ? 1 : 0 // Convierte a int
            };

            return Json(vmProducto); // Retorna el producto como JSON
        }
    }
}

