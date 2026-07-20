using ControlTaxisApp.Models;
using ControlTaxisApp.Models.ViewModels;
using ControlTaxisApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ControlTaxisApp.Controllers
{
    public class ReportesController : Controller
    {
        private readonly ControlTaxisContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ReportesController(ControlTaxisContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private string UserId => _httpContextAccessor.HttpContext?.User?.Identity?.Name;

        public async Task<IActionResult> Index(string? placaFiltro, int? mes, int? anio, DateTime? fechaInicio,
                                 DateTime? fechaFin, string? estadoFiltro, string? tipoDiaFiltro)
        {
            // 1. Instanciamos el modelo una sola vez
            var model = new ReporteConsolidadoViewModel();

            var listaFestivos = await _context.Festivos.Select(f => f.Fecha.Date).ToListAsync();
            ViewBag.Festivos = listaFestivos;

            // Filtramos las placas disponibles solo para este usuario
            ViewBag.Placas = await _context.Vehiculos
                .Where(v => v.UsuarioId == UserId)
                .Select(v => new SelectListItem { Value = v.Placa, Text = v.Placa })
                .ToListAsync();

            // 3. Consultas base filtradas por UsuarioId
            var liquidacionesQuery = _context.LiquidacionesDiarias
                .Include(l => l.Vehiculo)
                .Where(l => l.Vehiculo != null && l.Vehiculo.UsuarioId == UserId)
                .AsQueryable();

            var mantenimientosQuery = _context.Mantenimientos
                .Include(m => m.IdVehiculoNavigation)
                .Include(m => m.TipoMantenimiento)
                .Where(m => m.IdVehiculoNavigation != null && m.IdVehiculoNavigation.UsuarioId == UserId)
                .AsQueryable();

            var gastosQuery = _context.GastosAdministrativos
                .Where(g => _context.Vehiculos.Any(v => v.Placa == g.Placa && v.UsuarioId == UserId))
                .AsQueryable();
           
            // 4. Aplicar filtros a todas las tablas
            if (!string.IsNullOrEmpty(placaFiltro) && placaFiltro != "Todos")
            {
                liquidacionesQuery = liquidacionesQuery.Where(l => l.Vehiculo != null && l.Vehiculo.Placa == placaFiltro);
                mantenimientosQuery = mantenimientosQuery.Where(m => m.IdVehiculoNavigation != null && m.IdVehiculoNavigation.Placa == placaFiltro);
                gastosQuery = gastosQuery.Where(g => g.Placa == placaFiltro); // <-- Filtro de gastos
            }
            // FILTRO AÑO (Faltaba)
            if (anio.HasValue)
            {
                liquidacionesQuery = liquidacionesQuery.Where(l => l.Fecha.Year == anio.Value);
                mantenimientosQuery = mantenimientosQuery.Where(m => m.Fecha.Year == anio.Value);
                gastosQuery = gastosQuery.Where(g => g.Fecha.Year == anio.Value);
            }

            // FILTRO FECHAS (Faltaba)
            if (fechaInicio.HasValue && fechaFin.HasValue)
            {
                liquidacionesQuery = liquidacionesQuery.Where(l => l.Fecha.Date >= fechaInicio.Value.Date && l.Fecha.Date <= fechaFin.Value.Date);
                mantenimientosQuery = mantenimientosQuery.Where(m => m.Fecha.Date >= fechaInicio.Value.Date && m.Fecha.Date <= fechaFin.Value.Date);
                gastosQuery = gastosQuery.Where(g => g.Fecha.Date >= fechaInicio.Value.Date && g.Fecha.Date <= fechaFin.Value.Date);
            }
            // ... (mantén tus filtros de mes, año y fecha igual, aplicándolos también a gastosQuery) ...
            if (mes.HasValue)
            {
                liquidacionesQuery = liquidacionesQuery.Where(l => l.Fecha.Month == mes);
                mantenimientosQuery = mantenimientosQuery.Where(m => m.Fecha.Month == mes);
                gastosQuery = gastosQuery.Where(g => g.Fecha.Month == mes);
            }

            // 5. Ejecutar consultas
            model.Liquidaciones = await liquidacionesQuery.ToListAsync();
            model.Mantenimientos = await mantenimientosQuery.ToListAsync();
            model.GastosAdministrativos = await gastosQuery.ToListAsync(); 

            // 6. Lógica de tipo día (se mantiene igual para las liquidaciones)
            if (!string.IsNullOrEmpty(tipoDiaFiltro) && tipoDiaFiltro != "Todos")
            {
                if (tipoDiaFiltro == "Festivo")
                    model.Liquidaciones = model.Liquidaciones.Where(l => listaFestivos.Contains(l.Fecha.Date)).ToList();
                else if (tipoDiaFiltro == "Domingo")
                    model.Liquidaciones = model.Liquidaciones.Where(l => l.Fecha.DayOfWeek == DayOfWeek.Sunday).ToList();
                else if (tipoDiaFiltro == "Habil")
                    model.Liquidaciones = model.Liquidaciones.Where(l => l.Fecha.DayOfWeek != DayOfWeek.Sunday && !listaFestivos.Contains(l.Fecha.Date)).ToList();
            }

            return View(model);
        }

        private IQueryable<LiquidacionDiaria> AplicarFiltros(string? placaFiltro, int? mes, int? anio, DateTime? fechaInicio, DateTime? fechaFin, string? estadoFiltro)
        {
            var query = _context.LiquidacionesDiarias.Include(l => l.Vehiculo).AsQueryable();

            if (!string.IsNullOrEmpty(placaFiltro) && placaFiltro != "Todos")
                query = query.Where(l => l.Vehiculo != null && l.Vehiculo.Placa == placaFiltro);

            if (mes.HasValue) query = query.Where(l => l.Fecha.Month == mes);
            if (anio.HasValue) query = query.Where(l => l.Fecha.Year == anio);

            if (fechaInicio.HasValue && fechaFin.HasValue)
                query = query.Where(l => l.Fecha.Date >= fechaInicio.Value.Date && l.Fecha.Date <= fechaFin.Value.Date);

            if (!string.IsNullOrEmpty(estadoFiltro) && estadoFiltro != "Todos")
                query = query.Where(l => l.EstadoDia == estadoFiltro);

            return query;
        }

        public async Task<IActionResult> ExportarExcel(string? placaFiltro, int? mes, int? anio, DateTime? fechaInicio,
                                      DateTime? fechaFin, string? estadoFiltro, string? tipoDiaFiltro)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var userIdActual = user?.FindFirstValue(ClaimTypes.Name); // O Name, dependiendo de tu configuración
            // 1. Obtenemos los Ids y las Placas que pertenecen al usuario
            var misVehiculos = await _context.Vehiculos
                .Where(v => v.UsuarioId == userIdActual)
                .ToListAsync();

            var misVehiculosIds = misVehiculos.Select(v => v.Id).ToList();
            var misPlacas = misVehiculos.Select(v => v.Placa).ToList();

            // 2. Liquidaciones: Filtramos por los Ids del usuario y aplicamos filtros normales
            var query = AplicarFiltros(placaFiltro, mes, anio, fechaInicio, fechaFin, estadoFiltro)
                             .Where(l => misVehiculosIds.Contains(l.VehiculoId));

            var listaLiquidaciones = await query.ToListAsync();

            // Filtro Tipo de Día (en memoria)
            var listaFestivos = await _context.Festivos.Select(f => f.Fecha.Date).ToListAsync();
            // ... (tu lógica de filtros de listaLiquidaciones permanece igual)

            // 3. Mantenimientos: Filtramos por los Ids del usuario y luego filtros normales
            var queryMant = _context.Mantenimientos.Include(m => m.IdVehiculoNavigation)
                                                  .Include(m => m.TipoMantenimiento)
                                                  .Where(m => misVehiculosIds.Contains(m.VehiculoId))
                                                  .AsQueryable();

            // 4. Gastos Administrativos: Filtramos por las placas del usuario y luego filtros normales
            var queryGastos = _context.GastosAdministrativos
                                                  .Where(g => misPlacas.Contains(g.Placa))
                                                  .AsQueryable();

            // Aplicar filtros comunes a ambas listas
            if (!string.IsNullOrEmpty(placaFiltro) && placaFiltro != "Todos")
            {
                queryMant = queryMant.Where(m => m.IdVehiculoNavigation != null && m.IdVehiculoNavigation.Placa == placaFiltro);
                queryGastos = queryGastos.Where(g => g.Placa == placaFiltro);
            }

            if (mes.HasValue)
            {
                queryMant = queryMant.Where(m => m.Fecha.Month == mes);
                queryGastos = queryGastos.Where(g => g.Fecha.Month == mes);
            }

            if (anio.HasValue)
            {
                queryMant = queryMant.Where(m => m.Fecha.Year == anio);
                queryGastos = queryGastos.Where(g => g.Fecha.Year == anio);
            }

            if (fechaInicio.HasValue && fechaFin.HasValue)
            {
                queryMant = queryMant.Where(m => m.Fecha.Date >= fechaInicio.Value.Date && m.Fecha.Date <= fechaFin.Value.Date);
                queryGastos = queryGastos.Where(g => g.Fecha.Date >= fechaInicio.Value.Date && g.Fecha.Date <= fechaFin.Value.Date);
            }

            var listaMant = await queryMant.ToListAsync();
            var listaGastos = await queryGastos.ToListAsync();

            // 5. Generar Excel
            var service = new ReporteService();
            var fileBytes = service.GenerarExcel(listaLiquidaciones, listaMant, listaFestivos, listaGastos);

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Reporte_General.xlsx");
        }


    }
}