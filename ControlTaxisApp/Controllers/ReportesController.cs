using ControlTaxisApp.Models;
using ControlTaxisApp.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ControlTaxisApp.Controllers
{
    public class ReportesController : Controller
    {
        private readonly ControlTaxisContext _context;

        public ReportesController(ControlTaxisContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? placaFiltro, int? mes, int? anio, DateTime? fechaInicio, DateTime? fechaFin, string? picoYPlacaFiltro, string? tipoDiaFiltro)
        {
            // 1. Carga de datos base
            ViewBag.Festivos = await _context.Festivos.Select(f => f.Fecha.Date).ToListAsync();
            ViewBag.Placas = await _context.Vehiculos.Select(v => new SelectListItem { Value = v.Placa, Text = v.Placa }).ToListAsync();

            // 2. Unificamos la consulta (usamos liquidaciones como base principal)
            var liquidacionesQuery = _context.LiquidacionesDiarias.Include(l => l.Vehiculo).AsQueryable();
            var mantenimientosQuery = _context.Mantenimientos.Include(m => m.IdVehiculoNavigation).AsQueryable();

            // 3. Aplicar Filtros de Placa, Mes, Año y Rango
            if (!string.IsNullOrEmpty(placaFiltro))
            {
                liquidacionesQuery = liquidacionesQuery.Where(l => l.Vehiculo != null && l.Vehiculo.Placa == placaFiltro);
                mantenimientosQuery = mantenimientosQuery.Where(m => m.IdVehiculoNavigation != null && m.IdVehiculoNavigation.Placa == placaFiltro);
            }
            if (mes.HasValue)
            {
                liquidacionesQuery = liquidacionesQuery.Where(l => l.Fecha.Month == mes);
                mantenimientosQuery = mantenimientosQuery.Where(m => m.Fecha.Month == mes);
            }
            if (anio.HasValue)
            {
                liquidacionesQuery = liquidacionesQuery.Where(l => l.Fecha.Year == anio);
                mantenimientosQuery = mantenimientosQuery.Where(m => m.Fecha.Year == anio);
            }
            if (fechaInicio.HasValue && fechaFin.HasValue)
            {
                liquidacionesQuery = liquidacionesQuery.Where(l => l.Fecha.Date >= fechaInicio.Value.Date && l.Fecha.Date <= fechaFin.Value.Date);
                mantenimientosQuery = mantenimientosQuery.Where(m => m.Fecha.Date >= fechaInicio.Value.Date && m.Fecha.Date <= fechaFin.Value.Date);
            }

            // 4. Aplicar Filtros Especiales (Pico y Placa / Tipo de Día)
            // IMPORTANTE: Primero ejecutamos los filtros sobre la lista en memoria si es necesario 
            // o aplicamos la lógica sobre la query unificada.

            if (!string.IsNullOrEmpty(picoYPlacaFiltro) || !string.IsNullOrEmpty(tipoDiaFiltro))
            {
                var listaLiquidaciones = await liquidacionesQuery.ToListAsync();
                var listaFestivos = await _context.Festivos.Select(f => f.Fecha.Date).ToListAsync();

                if (!string.IsNullOrEmpty(picoYPlacaFiltro))
                {
                    bool filtrarPico = picoYPlacaFiltro == "true";
                    listaLiquidaciones = listaLiquidaciones.Where(l => LiquidacionesController.EsPicoYPlaca(l.Vehiculo?.Placa, l.Fecha) == filtrarPico).ToList();
                }

                if (!string.IsNullOrEmpty(tipoDiaFiltro))
                {
                    if (tipoDiaFiltro == "Festivo")
                        listaLiquidaciones = listaLiquidaciones.Where(l => listaFestivos.Contains(l.Fecha.Date)).ToList();
                    else if (tipoDiaFiltro == "Domingo")
                        listaLiquidaciones = listaLiquidaciones.Where(l => l.Fecha.DayOfWeek == DayOfWeek.Sunday).ToList();
                    else if (tipoDiaFiltro == "Habil")
                        listaLiquidaciones = listaLiquidaciones.Where(l => l.Fecha.DayOfWeek != DayOfWeek.Sunday && !listaFestivos.Contains(l.Fecha.Date)).ToList();
                }

                var modelConFiltros = new ReporteConsolidadoViewModel
                {
                    Liquidaciones = listaLiquidaciones,
                    Mantenimientos = await mantenimientosQuery.ToListAsync()
                };
                return View(modelConFiltros);
            }

            // 5. Retorno estándar si no hay filtros especiales
            var model = new ReporteConsolidadoViewModel
            {
                Liquidaciones = await liquidacionesQuery.ToListAsync(),
                Mantenimientos = await mantenimientosQuery.ToListAsync()
            };

            return View(model);
        }
    }
}