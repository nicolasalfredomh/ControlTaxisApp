using ControlTaxisApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Globalization;

namespace ControlTaxisApp.Controllers
{
    public class MantenimientosController : Controller
    {
        private readonly ControlTaxisContext _context;
        private readonly string _userId;
        public MantenimientosController(ControlTaxisContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            // Obtenemos el nombre de usuario igual que en el LiquidacionesController
            _userId = httpContextAccessor.HttpContext?.User.Identity?.Name;
        }

        // Método auxiliar para verificar propiedad
        private async Task<bool> EsPropietario(int vehiculoId)
        {
            return await _context.Vehiculos
                                 .AnyAsync(v => v.Id == vehiculoId && v.UsuarioId == _userId);
        }

        public async Task<IActionResult> Index(string? placaFiltro, int? tipoId)
        {
            var query = _context.Mantenimientos
                   .Include(m => m.IdVehiculoNavigation)
                   .Include(m => m.TipoMantenimiento)
                   .Where(m => m.IdVehiculoNavigation != null && m.IdVehiculoNavigation.UsuarioId == _userId)
                   .AsQueryable();

            if (!string.IsNullOrEmpty(placaFiltro))
            {
                query = query.Where(m => m.IdVehiculoNavigation != null && m.IdVehiculoNavigation.Placa == placaFiltro);
            }
            if (tipoId.HasValue)
            {
                query = query.Where(m => m.TipoMantenimientoId == tipoId);
            }

            // 4. Preparar datos para los selects
            ViewBag.TiposFiltro = await _context.TiposMantenimiento.ToListAsync();

            var listaMantenimientos = await query.OrderByDescending(m => m.Fecha).AsNoTracking().ToListAsync();

            // Filtramos las placas que se muestran en el SelectList
            var placasUsuario = await _context.Vehiculos.Where(v => v.UsuarioId == _userId).ToListAsync();
            ViewBag.Placas = new SelectList(placasUsuario, "Placa", "Placa", placaFiltro);
            return View(listaMantenimientos);
        }

        public async Task<IActionResult> Crear()
        {
            var vehiculos = await _context.Vehiculos.Where(v => v.UsuarioId == _userId).ToListAsync();
            ViewBag.Tipos = new SelectList(await _context.TiposMantenimiento.ToListAsync(), "Id", "Nombre");
            ViewBag.IdVehiculo = new SelectList(vehiculos, "Id", "Placa");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Mantenimiento nuevoMantenimiento)
        {
            if (!await EsPropietario(nuevoMantenimiento.VehiculoId)) return Forbid();

            if (ModelState.IsValid)
            {
                _context.Mantenimientos.Add(nuevoMantenimiento);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Tipos = new SelectList(await _context.TiposMantenimiento.ToListAsync(), "Id", "Nombre", nuevoMantenimiento.TipoMantenimientoId);
            ViewBag.IdVehiculo = new SelectList(await _context.Vehiculos.Where(v => v.UsuarioId == _userId).ToListAsync(), "Id", "Placa", nuevoMantenimiento.VehiculoId);
            return View(nuevoMantenimiento);
        }

        public async Task<IActionResult> Editar(int id)
        {
            var mantenimiento = await _context.Mantenimientos.Include(m => m.IdVehiculoNavigation).FirstOrDefaultAsync(m => m.Id == id);

            // Seguridad: verificar que el mantenimiento exista y pertenezca al usuario
            if (mantenimiento == null || mantenimiento.IdVehiculoNavigation?.UsuarioId != _userId) return NotFound();

            ViewBag.Tipos = new SelectList(await _context.TiposMantenimiento.ToListAsync(), "Id", "Nombre", mantenimiento.TipoMantenimientoId);
            ViewBag.IdVehiculo = new SelectList(await _context.Vehiculos.Where(v => v.UsuarioId == _userId).ToListAsync(), "Id", "Placa", mantenimiento.VehiculoId);

            return View(mantenimiento);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Mantenimiento mantModificado)
        {
            if (id != mantModificado.Id) return NotFound();

            if (!await EsPropietario(mantModificado.VehiculoId)) return Forbid();

            if (ModelState.IsValid)
            {
                _context.Update(mantModificado);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Tipos = new SelectList(await _context.TiposMantenimiento.ToListAsync(), "Id", "Nombre", mantModificado.TipoMantenimientoId);
            ViewBag.IdVehiculo = new SelectList(await _context.Vehiculos.Where(v => v.UsuarioId == _userId).ToListAsync(), "Id", "Placa", mantModificado.VehiculoId);
            return View(mantModificado);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var mantenimiento = await _context.Mantenimientos.Include(m => m.IdVehiculoNavigation).FirstOrDefaultAsync(m => m.Id == id);

            if (mantenimiento != null && mantenimiento.IdVehiculoNavigation?.UsuarioId == _userId)
            {
                _context.Mantenimientos.Remove(mantenimiento);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }




    }
}