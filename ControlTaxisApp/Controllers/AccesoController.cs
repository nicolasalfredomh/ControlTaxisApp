using Microsoft.AspNetCore.Mvc;
using ControlTaxisApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace ControlTaxisApp.Controllers
{
    public class AccesoController : Controller
    {
        private readonly ControlTaxisContext _context;
        private const string AuthScheme = "CookieAuth";

        public AccesoController(ControlTaxisContext context)
        {
            _context = context;
        }

        [HttpGet] 
        public IActionResult Registro() { return View(); }

        [HttpPost]
        public IActionResult Registro(Usuario nuevoUsuario)
        {
           
            // para ver si al dar clic en el botón, el código se detiene aquí.
            if (ModelState.IsValid)
            {
                _context.Usuarios.Add(nuevoUsuario);
                _context.SaveChanges();
                return RedirectToAction("Login");
            }
            return View(nuevoUsuario);
        }
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string nombreUsuario, string clave, string? returnUrl)
        {
            var usuarioEcontrado = await _context.Usuarios
                .FromSqlRaw("SELECT * FROM Usuarios WHERE LOWER(NombreUsuario) = LOWER({0}) AND Clave = {1}", nombreUsuario, clave)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (usuarioEcontrado != null)
            {
                var claims = new List<Claim> {
                    new Claim(ClaimTypes.Name, usuarioEcontrado.NombreCompleto ?? usuarioEcontrado.NombreUsuario),
                    new Claim("NombreUsuario", usuarioEcontrado.NombreUsuario)
                };

                var claimsIdentity = new ClaimsIdentity(claims, AuthScheme);
                await HttpContext.SignInAsync(AuthScheme, new ClaimsPrincipal(claimsIdentity));

                // 2. CORRECCIÓN: Si hay una URL de retorno, úsala, si no, ve a Home/Index
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Usuario o contraseña incorrectos.";
            return View();
        }

        public async Task<IActionResult> Salir()
        {
            await HttpContext.SignOutAsync(AuthScheme);
            return RedirectToAction("Login", "Acceso");
        }
    }
}