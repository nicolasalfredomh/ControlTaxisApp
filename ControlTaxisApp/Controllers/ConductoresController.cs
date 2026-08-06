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
        public async Task<IActionResult> GuardarHojaDeVida(Conductor model, IFormFile? archivoFoto, IFormFile? archivoLicenciaFrente, IFormFile? archivoLicenciaAtras)
        {
            if (model.VehiculoId == null) return NotFound();

            // Validar que el vehículo pertenezca al usuario actual
            var vehiculo = await _context.Vehiculos.FirstOrDefaultAsync(v => v.Id == model.VehiculoId && v.UsuarioId == _userId);
            if (vehiculo == null) return Forbid();

            string carpetaFOTOS = Path.Combine(_env.WebRootPath, "images", "conductores");
            if (!Directory.Exists(carpetaFOTOS)) Directory.CreateDirectory(carpetaFOTOS);

            // 1. Gestión de la fotografía principal del conductor
            if (archivoFoto != null && archivoFoto.Length > 0)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(archivoFoto.FileName);
                string filePath = Path.Combine(carpetaFOTOS, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await archivoFoto.CopyToAsync(stream);
                }

                model.FotoUrl = "/images/conductores/" + fileName;
            }

            // 2. Gestión de la foto de la licencia (Frente)
            if (archivoLicenciaFrente != null && archivoLicenciaFrente.Length > 0)
            {
                string fileNameFrente = "frente_" + Guid.NewGuid().ToString() + Path.GetExtension(archivoLicenciaFrente.FileName);
                string filePathFrente = Path.Combine(carpetaFOTOS, fileNameFrente);

                using (var stream = new FileStream(filePathFrente, FileMode.Create))
                {
                    await archivoLicenciaFrente.CopyToAsync(stream);
                }

                model.LicenciaFrenteUrl = "/images/conductores/" + fileNameFrente;
            }

            // 3. Gestión de la foto de la licencia (Atrás)
            if (archivoLicenciaAtras != null && archivoLicenciaAtras.Length > 0)
            {
                string fileNameAtras = "atras_" + Guid.NewGuid().ToString() + Path.GetExtension(archivoLicenciaAtras.FileName);
                string filePathAtras = Path.Combine(carpetaFOTOS, fileNameAtras);

                using (var stream = new FileStream(filePathAtras, FileMode.Create))
                {
                    await archivoLicenciaAtras.CopyToAsync(stream);
                }

                model.LicenciaAtrasUrl = "/images/conductores/" + fileNameAtras;
            }

            // 4. Buscar si ya existe un conductor para este vehículo en la Base de Datos
            var conductorExistente = await _context.Conductores.FirstOrDefaultAsync(c => c.VehiculoId == model.VehiculoId);

            if (conductorExistente != null)
            {
                // Actualizar datos personales y de contacto
                conductorExistente.Nombre = model.Nombre;
                conductorExistente.Apellidos = model.Apellidos;
                conductorExistente.Telefono = model.Telefono;
                conductorExistente.Direccion = model.Direccion;
                conductorExistente.Activo = model.Activo;

                conductorExistente.ContactoNombre = model.ContactoNombre;
                conductorExistente.ContactoApellidos = model.ContactoApellidos;
                conductorExistente.ContactoCorreo = model.ContactoCorreo;
                conductorExistente.ContactoTelefono = model.ContactoTelefono;

                // Conservar o actualizar imágenes según corresponda
                if (!string.IsNullOrEmpty(model.FotoUrl))
                {
                    conductorExistente.FotoUrl = model.FotoUrl;
                }
                if (!string.IsNullOrEmpty(model.LicenciaFrenteUrl))
                {
                    conductorExistente.LicenciaFrenteUrl = model.LicenciaFrenteUrl;
                }
                if (!string.IsNullOrEmpty(model.LicenciaAtrasUrl))
                {
                    conductorExistente.LicenciaAtrasUrl = model.LicenciaAtrasUrl;
                }

                _context.Conductores.Update(conductorExistente);
            }
            else
            {
                // Agregar como nuevo registro
                _context.Conductores.Add(model);
            }

            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "¡Hoja de vida guardada con éxito!";

            return RedirectToAction("Index", "Vehiculos");
        }


    }
}