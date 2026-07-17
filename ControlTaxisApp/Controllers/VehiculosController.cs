using Microsoft.AspNetCore.Mvc;
using ControlTaxisApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ControlTaxisApp.Controllers
{
    public class VehiculosController : Controller
    {
        private readonly ControlTaxisContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public VehiculosController(ControlTaxisContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        // Propiedad dinámica para obtener el usuario actual
        private string UserId => _httpContextAccessor.HttpContext?.User?.Identity?.Name;

        // GET: Muestra la lista solo de los vehículos del usuario
        public async Task<IActionResult> Index()
        {
            if (string.IsNullOrEmpty(UserId)) return RedirectToAction("Login", "Account");

            var listaVehiculos = await _context.Vehiculos
                .Where(v => v.UsuarioId == UserId)
                .AsNoTracking()
                .ToListAsync();

            return View(listaVehiculos);
        }


        // 1. GET: Muestra el formulario vacío para crear un vehículo
        public IActionResult Crear()
        {
            return View();
        }

        // POST: Crea el vehículo asignándole el UserId actual
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Vehiculo nuevoVehiculo)
        {
            // Asignamos el campo que valida tu sistema (NombreCompleto/UsuarioId)
            nuevoVehiculo.UsuarioId = UserId;

            if (ModelState.IsValid)
            {
                _context.Vehiculos.Add(nuevoVehiculo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(nuevoVehiculo);
        }
        // GET: Edita un vehículo, validando que sea del usuario
        public async Task<IActionResult> Editar(int id)
        {
            var vehiculo = await _context.Vehiculos
                .FirstOrDefaultAsync(v => v.Id == id && v.UsuarioId == UserId);

            if (vehiculo == null) return NotFound();

            return View(vehiculo);
        }
        // POST: Guarda los cambios, manteniendo el UsuarioId
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Vehiculo vehiculoModificado)
        {
            if (id != vehiculoModificado.Id) return NotFound();

            // Aseguramos que el registro original pertenece al usuario
            var original = await _context.Vehiculos.FirstOrDefaultAsync(v => v.Id == id && v.UsuarioId == UserId);
            if (original == null) return NotFound();

            // Mantenemos el UsuarioId original para no perder la propiedad
            vehiculoModificado.UsuarioId = UserId;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Entry(original).CurrentValues.SetValues(vehiculoModificado);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(vehiculoModificado);
        }

        // POST: Elimina el vehículo si no tiene registros asociados
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var vehiculo = await _context.Vehiculos.FirstOrDefaultAsync(v => v.Id == id && v.UsuarioId == UserId);

            if (vehiculo == null) return NotFound();

            bool tieneMantenimientos = await _context.Mantenimientos.AnyAsync(m => m.VehiculoId == id);
            bool tieneLiquidaciones = await _context.LiquidacionesDiarias.AnyAsync(l => l.VehiculoId == id);

            if (tieneMantenimientos || tieneLiquidaciones)
            {
                TempData["ErrorEliminacion"] = "No se puede eliminar el vehículo porque tiene liquidaciones o mantenimientos asociados.";
                return RedirectToAction(nameof(Index));
            }

            _context.Vehiculos.Remove(vehiculo);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}