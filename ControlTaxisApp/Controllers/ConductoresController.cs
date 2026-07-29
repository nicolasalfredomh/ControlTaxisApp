using ControlTaxisApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ControlTaxisApp.Controllers
{
    public class ConductoresController : Controller
    {
        private readonly ControlTaxisContext _context;
        private readonly string _userId;
        private readonly IWebHostEnvironment _env;

        public ConductoresController(ControlTaxisContext context, IHttpContextAccessor httpContextAccessor, IWebHostEnvironment env)
        {
            _context = context;
            _userId = httpContextAccessor.HttpContext?.User.Identity?.Name;
            _env = env;
        }

        // GET: Conductores/HojaDeVida/5 (Donde 5 es el Id del Vehículo)
        public async Task<IActionResult> HojaDeVida(int vehiculoId)
        {
            var vehiculo = await _context.Vehiculos.FirstOrDefaultAsync(v => v.Id == vehiculoId && v.UsuarioId == _userId);
            if (vehiculo == null) return NotFound();

            // Buscamos si ya existe un conductor vinculado a este vehículo
            var conductor = await _context.Conductores.FirstOrDefaultAsync(c => c.VehiculoId == vehiculoId);

            if (conductor == null)
            {
                // Si no existe, creamos una instancia nueva asegurando Id = 0 y su VehiculoId
                conductor = new Conductor
                {
                    Id = 0,
                    VehiculoId = vehiculoId,
                    Activo = true
                };
            }

            ViewBag.PlacaVehiculo = vehiculo.Placa;
            return View(conductor);
        }

        // POST: Conductores/GuardarHojaDeVida
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarHojaDeVida(Conductor model, IFormFile? archivoFoto)
        {
            if (model.VehiculoId == null) return NotFound();

            // Validar que el vehículo pertenezca al usuario actual
            var vehiculo = await _context.Vehiculos.FirstOrDefaultAsync(v => v.Id == model.VehiculoId && v.UsuarioId == _userId);
            if (vehiculo == null) return Forbid();

            // 1. Gestión de la fotografía
            if (archivoFoto != null && archivoFoto.Length > 0)
            {
                string carpetaFOTOS = Path.Combine(_env.WebRootPath, "images", "conductores");
                if (!Directory.Exists(carpetaFOTOS)) Directory.CreateDirectory(carpetaFOTOS);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(archivoFoto.FileName);
                string filePath = Path.Combine(carpetaFOTOS, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await archivoFoto.CopyToAsync(stream);
                }

                model.FotoUrl = "/images/conductores/" + fileName;
            }

            // 2. Buscar si ya existe un conductor para este vehículo en la Base de Datos
            var conductorExistente = await _context.Conductores.FirstOrDefaultAsync(c => c.VehiculoId == model.VehiculoId);

            if (conductorExistente != null)
            {
                // Si ya existe, actualizamos sus propiedades para no depender únicamente del ID del formulario
                conductorExistente.Nombre = model.Nombre;
                conductorExistente.Apellidos = model.Apellidos;
                conductorExistente.Telefono = model.Telefono;
                conductorExistente.Direccion = model.Direccion;
                conductorExistente.Activo = model.Activo;

                if (!string.IsNullOrEmpty(model.FotoUrl))
                {
                    conductorExistente.FotoUrl = model.FotoUrl; // Solo reemplazamos si subió una nueva foto
                }

                _context.Conductores.Update(conductorExistente);
            }
            else
            {
                // Si no existe ninguno, lo agregamos como nuevo
                _context.Conductores.Add(model);
            }

            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "¡Hoja de vida guardada con éxito!";

            // Redirige de vuelta a la lista de vehículos o donde prefieras
            return RedirectToAction("Index", "Vehiculos");
        }
    }
}