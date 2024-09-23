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


    }
}
