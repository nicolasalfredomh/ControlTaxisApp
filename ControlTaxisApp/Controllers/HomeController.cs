using ControlTaxisApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ControlTaxisApp.Controllers
{
    public class HomeController : Controller
    {
        // 1. Agregamos el campo privado para el contexto
        private readonly ControlTaxisContext _context;

        // 2. Agregamos el constructor para inyectar el contexto de la base de datos
        public HomeController(ControlTaxisContext context)
        {
            _context = context;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            // Obtenemos todos los vehículos desde la base de datos
            var todosLosVehiculos = await _context.Vehiculos.ToListAsync();

            // Filtramos usando nuestro servicio de Pico y Placa
            var vehiculosConRestriccion = todosLosVehiculos
                .Where(v => ControlTaxisApp.Services.PicoYPlacaService.TienePicoYPlaca(v.Placa, DateTime.Now))
                .ToList();

            ViewBag.VehiculosRestringidos = vehiculosConRestriccion;

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}