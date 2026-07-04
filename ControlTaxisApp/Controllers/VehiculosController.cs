using Microsoft.AspNetCore.Mvc;
using ControlTaxisApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ControlTaxisApp.Controllers
{
    public class VehiculosController : Controller
    {
        private readonly ControlTaxisContext _context;

        public VehiculosController(ControlTaxisContext context)
        {
            _context = context;
        }

        // GET: Muestra la lista de todos los taxis
        public async Task<IActionResult> Index()
        {
            var listaVehiculos = await _context.Vehiculos
                .AsNoTracking()
                .ToListAsync();

            return View(listaVehiculos);
        }


        // 1. GET: Muestra el formulario vacío para crear un vehículo
        public IActionResult Crear()
        {
            return View();
        }

        // 2. POST: Recibe los datos del formulario y los guarda en la BD
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Vehiculo nuevoVehiculo)
        {
            if (ModelState.IsValid)
            {
                _context.Vehiculos.Add(nuevoVehiculo);
                await _context.SaveChangesAsync();

                // Al guardar con éxito, volvemos a la tabla
                return RedirectToAction(nameof(Index));
            }

            // Si el modelo tiene algún error, recarga la vista con los datos actuales
            return View(nuevoVehiculo);
        }


        // 1. GET: Busca el vehículo por ID y abre el formulario de edición
        public async Task<IActionResult> Editar(int id)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(id);
            if (vehiculo == null)
            {
                return NotFound();
            }
            return View(vehiculo);
        }

        // 2. POST: Recibe el vehículo editado y guarda los cambios en SQL Server
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Vehiculo vehiculoModificado)
        {
            if (id != vehiculoModificado.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vehiculoModificado);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Vehiculos.Any(e => e.Id == vehiculoModificado.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(vehiculoModificado);
        }

        // 3. POST: Elimina el vehículo físicamente por su ID
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            // 1. Verificamos las relaciones
            bool tieneMantenimientos = await _context.Mantenimientos.AnyAsync(m => m.VehiculoId == id);
            bool tieneLiquidaciones = await _context.LiquidacionesDiarias.AnyAsync(l => l.VehiculoId == id);

            if (tieneMantenimientos || tieneLiquidaciones)
            {
                // Guardamos el mensaje en TempData para que la vista lo lea
                TempData["ErrorEliminacion"] = "No se puede eliminar el vehículo porque tiene liquidaciones o mantenimientos asociados.";
                return RedirectToAction(nameof(Index));
            }

            // 2. Si no tiene relaciones, procedemos a borrar
            var vehiculo = await _context.Vehiculos.FindAsync(id);
            if (vehiculo != null)
            {
                _context.Vehiculos.Remove(vehiculo);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }


    }
}