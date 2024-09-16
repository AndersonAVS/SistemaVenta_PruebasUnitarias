using Microsoft.AspNetCore.Mvc;

using SistemaVenta.AplicacionWeb.Models.ViewModels;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.Entity;

//seguridad de autenticacion por cookies
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

        //se tienen que activar toda la configuracion de las cookies en Program.cs 
        public IActionResult Login()
        {
            //se debe especificar si es que el usuario ya inició sesion ya no debería mostrar la vista de login sino que debería redireccionar hacia el formulario de Home
            ClaimsPrincipal claimUser = HttpContext.User; //aqui se guarda la sesion del usuario

            //validar si es que existe un usuario legeado o no
            if (claimUser.Identity.IsAuthenticated) {
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

            Usuario usuario_encontrado = await _usuarioServicio.ObtenerPorCredenciales(modelo.Correo, modelo.Clave);
            //un viewdata nos permite compartir información desde nuestro controlador a la vista
            if (usuario_encontrado == null) {
                ViewData["Mensaje"] = "No se encontraron coincidencias";
                return View();
            }

            //Si el usuario si ha sido encontrado
            ViewData["Mensaje"] = null;

            List<Claim> claims = new List<Claim>() { 
                new Claim(ClaimTypes.Name, usuario_encontrado.Nombre),
                new Claim(ClaimTypes.NameIdentifier, usuario_encontrado.IdUsuario.ToString()),
                new Claim(ClaimTypes.Role, usuario_encontrado.IdRol.ToString()),
                new Claim("UrlFoto", usuario_encontrado.UrlFoto),
            };

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            AuthenticationProperties properties = new AuthenticationProperties() { 
                AllowRefresh = true,
                IsPersistent = modelo.MantenerSesion
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                properties
                );

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> RestablecerClave(VMUsuarioLogin modelo)
        {

            try
            {
                string urlPlantillaCorreo = $"{this.Request.Scheme}://{this.Request.Host}/Plantilla/RestablecerClave?clave=[clave]";

                bool resultado = await _usuarioServicio.RestablecerClave(modelo.Correo, urlPlantillaCorreo);

                if (resultado) {
                    ViewData["Mensaje"] = "Listo, su contraseña fue restablecida. Revise su correo";
                    ViewData["MensajeError"] = null;
                }
                else {
                    ViewData["MensajeError"] = "Hubo un problema. Por favor inténtelo de nuevo más tarde";
                    ViewData["Mensaje"] = null;
                }

            }
            catch(Exception ex) {
                ViewData["MensajeError"] =ex.Message;
                ViewData["Mensaje"] = null;
            }

            return View();
        }

    }
}
