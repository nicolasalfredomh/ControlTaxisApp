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

        public async Task<IActionResult> Index(string? placaFiltro, int? mes, int? anio, DateTime? fechaInicio,
                                         DateTime? fechaFin, string? estadoFiltro, string? tipoDiaFiltro)
        {
            // 1. Carga de datos base
            var listaFestivos = await _context.Festivos.Select(f => f.Fecha.Date).ToListAsync();
            ViewBag.Festivos = listaFestivos;
            ViewBag.Placas = await _context.Vehiculos.Select(v => new SelectListItem { Value = v.Placa, Text = v.Placa }).ToListAsync();

            // 2. Definimos las consultas base
            var liquidacionesQuery = _context.LiquidacionesDiarias.Include(l => l.Vehiculo).AsQueryable();
            var mantenimientosQuery = _context.Mantenimientos.Include(m => m.IdVehiculoNavigation).AsQueryable();

            // 3. Filtros básicos (se aplican siempre a la IQueryable)
            if (!string.IsNullOrEmpty(placaFiltro) && placaFiltro != "Todos")
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

            // 4. Aplicar Filtro de Estado (Manual)
            if (!string.IsNullOrEmpty(estadoFiltro) && estadoFiltro != "Todos")
            {
                liquidacionesQuery = liquidacionesQuery.Where(l => l.EstadoDia == estadoFiltro);
            }

            // 5. Ejecutamos la consulta para obtener los datos
            var listaLiquidaciones = await liquidacionesQuery.ToListAsync();
            var listaMantenimientos = await mantenimientosQuery.ToListAsync();

            // 6. Aplicar Filtro de Tipo de Día (Lógica basada en fecha, se hace en memoria)
            if (!string.IsNullOrEmpty(tipoDiaFiltro) && tipoDiaFiltro != "Todos")
            {
                if (tipoDiaFiltro == "Festivo")
                    listaLiquidaciones = listaLiquidaciones.Where(l => listaFestivos.Contains(l.Fecha.Date)).ToList();
                else if (tipoDiaFiltro == "Domingo")
                    listaLiquidaciones = listaLiquidaciones.Where(l => l.Fecha.DayOfWeek == DayOfWeek.Sunday).ToList();
                else if (tipoDiaFiltro == "Habil")
                    listaLiquidaciones = listaLiquidaciones.Where(l => l.Fecha.DayOfWeek != DayOfWeek.Sunday && !listaFestivos.Contains(l.Fecha.Date)).ToList();
            }

            var model = new ReporteConsolidadoViewModel
            {
                Liquidaciones = listaLiquidaciones,
                Mantenimientos = listaMantenimientos
            };

            return View(model);
        }


    }
}