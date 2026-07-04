using ControlTaxisApp.Models; // <--- ASEGÚRATE DE QUE ESTE SEA EL NAMESPACE DONDE VIVE TU CONTEXTO
using ControlTaxisApp.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ControlTaxisApp.Controllers
{
    public class ReportesController : Controller
    {
  
        private readonly ControlTaxisContext _context;

        public ReportesController(ControlTaxisContext context)
        {
            _context = context;

        }

        public async Task<IActionResult> Index(string placaFiltro, int? mes, int? anio, DateTime? fechaInicio, DateTime? fechaFin)
        {
            // Carga de placas para el select (mantener igual)
            ViewBag.Placas = await _context.Vehiculos.Select(v => new SelectListItem { Value = v.Placa, Text = v.Placa }).ToListAsync();

            var liquidaciones = _context.LiquidacionesDiarias.Include(l => l.Vehiculo).AsQueryable();
            var mantenimientos = _context.Mantenimientos.Include(m => m.IdVehiculoNavigation).AsQueryable();

            // Filtros dinámicos
            if (!string.IsNullOrEmpty(placaFiltro))
            {
                liquidaciones = liquidaciones.Where(l => l.Vehiculo != null && l.Vehiculo.Placa == placaFiltro);
                mantenimientos = mantenimientos.Where(m => m.IdVehiculoNavigation != null && m.IdVehiculoNavigation.Placa == placaFiltro);
            }
            if (mes.HasValue)
            {
                liquidaciones = liquidaciones.Where(l => l.Fecha.Month == mes); // Asegúrate de que tu modelo tenga 'Fecha'
                mantenimientos = mantenimientos.Where(m => m.Fecha.Month == mes);
            }
            if (anio.HasValue)
            {
                liquidaciones = liquidaciones.Where(l => l.Fecha.Year == anio);
                mantenimientos = mantenimientos.Where(m => m.Fecha.Year == anio);
            }
            if (fechaInicio.HasValue && fechaFin.HasValue)
            {
                liquidaciones = liquidaciones.Where(l => l.Fecha >= fechaInicio && l.Fecha <= fechaFin);
                mantenimientos = mantenimientos.Where(m => m.Fecha >= fechaInicio && m.Fecha <= fechaFin);
            }

            var model = new ReporteConsolidadoViewModel
            {
                Liquidaciones = await liquidaciones.ToListAsync(),
                Mantenimientos = await mantenimientos.ToListAsync()
            };

            return View(model);
        }



    }
}