using Microsoft.AspNetCore.Mvc;
using SistemaVenta.AplicacionWeb.Models.ViewModels;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.Entity;

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;


namespace SistemaVenta.AplicacionWeb.Controllers
{
    public class AccesoController : Controller
    {
        private readonly IUsuarioService _usuarioServicio;

        public AccesoController(IUsuarioService usuarioServicio)
        {
            _usuarioServicio = usuarioServicio;
        }
        

        public IActionResult Login()
        {
            ClaimsPrincipal claimUser = HttpContext.User;

            if (claimUser.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        public IActionResult RestablecerClave()
        {



            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Login(VMUsuarioLogin modelo)
        {
            // Validación de los campos de correo y clave
            if (string.IsNullOrEmpty(modelo.Correo) || string.IsNullOrEmpty(modelo.Clave))
            {
                ViewData["Mensaje"] = "Debe ingresar tanto el correo como la clave.";
                return View();
            }

            // Buscar usuario por las credenciales proporcionadas
            Usuario usuario_encontrado = await _usuarioServicio.ObtenerPorCredenciales(modelo.Correo, modelo.Clave);

            // Verificación de si el usuario fue encontrado
            if (usuario_encontrado == null)
            {
                ViewData["Mensaje"] = "No se encontraron coincidencias.";
                return View();
            }

            ViewData["Mensaje"] = null;

            // Crear la lista de claims con la información del usuario
            List<Claim> claims = new List<Claim>()
    {
        new Claim(ClaimTypes.Name, usuario_encontrado.Nombre),
        new Claim(ClaimTypes.NameIdentifier, usuario_encontrado.IdUsuario.ToString()),
        new Claim(ClaimTypes.Role, usuario_encontrado.IdRol.ToString()),
        new Claim("UrlFoto", usuario_encontrado.UrlFoto)
    };

            // Crear la identidad de autenticación
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // Configurar las propiedades de autenticación
            AuthenticationProperties properties = new AuthenticationProperties()
            {
                AllowRefresh = true,
                IsPersistent = modelo.MantenerSesion
            };

            // Iniciar sesión con la autenticación basada en cookies
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                properties
            );

            // Redirigir al inicio una vez que el usuario haya iniciado sesión
            return RedirectToAction("Index", "Home");
        }


        [HttpPost]
        public async Task<IActionResult> RestablecerClave(VMUsuarioLogin modelo)
        {
            try
            {
                string urlPlantillaCorreo = $"{this.Request.Scheme}://{this.Request.Host}/Plantilla/RestablecerClave?clave=[clave]";

                bool resultado = await _usuarioServicio.RestablecerClave(modelo.Correo, urlPlantillaCorreo);

                if (resultado)
                {
                    ViewData["Mensaje"] = "Listo, su contraseña fue restablecida. Revise su correo";
                    ViewData["MensajeError"] = null;
                }
                else
                {
                    ViewData["MensajeError"] = "Tenemos problemas. Por fabor intentelo de nuevo mas tarde";
                    ViewData["Mensaje"] = null;
                }

            }
            catch (Exception ex) {
                ViewData["MensajeError"] = ex.Message;
                ViewData["Mensaje"] = null;

            }



            return View();
        }

    }
}
