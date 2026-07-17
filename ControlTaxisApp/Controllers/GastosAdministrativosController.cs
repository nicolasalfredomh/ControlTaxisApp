using ControlTaxisApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ControlTaxisApp.Controllers
{
    public class GastosAdministrativosController : Controller
    {
        private readonly ControlTaxisContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GastosAdministrativosController(ControlTaxisContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        // Propiedad dinámica que siempre devuelve el usuario actual
        private string UserId => _httpContextAccessor.HttpContext?.User?.Identity?.Name;

        // Método auxiliar para verificar propiedad
        private async Task<bool> EsPropietario(string placa)
        {
            return await _context.Vehiculos.AnyAsync(v => v.Placa == placa && v.UsuarioId == UserId);
        }

        public async Task<IActionResult> Index(string? placaFiltro, string? categoriaFiltro)
        {
            var query = _context.GastosAdministrativos
                                .Where(g => _context.Vehiculos.Any(v => v.Placa == g.Placa && v.UsuarioId == UserId))
                                .AsQueryable();

            if (!string.IsNullOrEmpty(placaFiltro)) query = query.Where(g => g.Placa == placaFiltro);
            if (!string.IsNullOrEmpty(categoriaFiltro)) query = query.Where(g => g.Concepto == categoriaFiltro);

            var listaGastos = await query.OrderByDescending(g => g.Fecha).AsNoTracking().ToListAsync();

            var listaPlacas = await _context.Vehiculos
                                       .Where(v => v.UsuarioId == UserId)
                                       .Select(v => v.Placa)
                                       .Distinct()
                                       .ToListAsync();

            // Usamos lista vacía o "Todas" para el filtro
            ViewBag.Placas = new SelectList(listaPlacas, placaFiltro);

            return View(listaGastos);
        }

        public async Task<IActionResult> Crear()
        {
            var placas = await _context.Vehiculos.Where(v => v.UsuarioId == UserId).Select(v => v.Placa).ToListAsync();
            ViewBag.Placas = new SelectList(placas);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(GastoAdministrativo nuevoGasto)
        {
            // Validar que el usuario sea dueño de la placa antes de guardar
            if (!await EsPropietario(nuevoGasto.Placa)) ModelState.AddModelError("Placa", "Vehículo no válido.");

            if (ModelState.IsValid)
            {
                _context.GastosAdministrativos.Add(nuevoGasto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var placas = await _context.Vehiculos.Where(v => v.UsuarioId == UserId).Select(v => v.Placa).ToListAsync();
            ViewBag.Placas = new SelectList(placas, nuevoGasto.Placa);
            return View(nuevoGasto);
        }

        public async Task<IActionResult> Editar(int id)
        {
            var gasto = await _context.GastosAdministrativos.FirstOrDefaultAsync(g => g.Id == id);
            if (gasto == null || !await EsPropietario(gasto.Placa)) return NotFound();

            var placas = await _context.Vehiculos.Where(v => v.UsuarioId == UserId).Select(v => v.Placa).ToListAsync();
            ViewBag.Placas = new SelectList(placas, gasto.Placa);
            return View(gasto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, GastoAdministrativo gastoModificado)
        {
            if (id != gastoModificado.Id || !await EsPropietario(gastoModificado.Placa)) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(gastoModificado);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var placas = await _context.Vehiculos.Where(v => v.UsuarioId == UserId).Select(v => v.Placa).ToListAsync();
            ViewBag.Placas = new SelectList(placas, gastoModificado.Placa);
            return View(gastoModificado);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var gasto = await _context.GastosAdministrativos.FindAsync(id);
            if (gasto != null && await EsPropietario(gasto.Placa))
            {
                _context.GastosAdministrativos.Remove(gasto);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}