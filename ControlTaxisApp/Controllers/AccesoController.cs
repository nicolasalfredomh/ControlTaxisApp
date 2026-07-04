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
        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Registro(Usuario nuevoUsuario)
        {
            if (ModelState.IsValid)
            {
                // Aquí podrías agregar lógica para encriptar la contraseña si la usas
                _context.Usuarios.Add(nuevoUsuario);
                _context.SaveChanges();
                return RedirectToAction("Login"); // Redirige al login tras registrarse
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
        public async Task<IActionResult> Login(string nombreUsuario, string clave)
        {
            if (string.IsNullOrEmpty(nombreUsuario) || string.IsNullOrEmpty(clave))
            {
                ViewBag.Error = "Por favor, complete todos los campos.";
                return View();
            }

            // MODO ULTRA SIMPLIFICADO: SQL evalúa usuario y contraseña al mismo tiempo.
            // No importa si la clave es '12345' o 'Admin123', lo que esté escrito tal cual en la columna 'Clave' pasará.
            var usuarioEcontrado = await _context.Usuarios
                .FromSqlRaw("SELECT * FROM Usuarios WHERE LOWER(NombreUsuario) = LOWER({0}) AND Clave = {1}", nombreUsuario, clave)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            // Si la consulta trajo algo, significa que los dos datos son correctos en la BD
            if (usuarioEcontrado != null)
            {
                var claims = new List<Claim> {
                    new Claim(ClaimTypes.Name, usuarioEcontrado.NombreCompleto ?? usuarioEcontrado.NombreUsuario),
                    new Claim("NombreUsuario", usuarioEcontrado.NombreUsuario)
                };

                var claimsIdentity = new ClaimsIdentity(claims, AuthScheme);
                await HttpContext.SignInAsync(AuthScheme, new ClaimsPrincipal(claimsIdentity));

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