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

        public MantenimientosController(ControlTaxisContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? placaFiltro, int? tipoId)
        {
            var query = _context.Mantenimientos
                .Include(m => m.IdVehiculoNavigation)
                .Include(m => m.TipoMantenimiento)
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

            ViewBag.Placas = new SelectList(await _context.Vehiculos.ToListAsync(), "Placa", "Placa", placaFiltro);

            return View(listaMantenimientos);
        }

        public async Task<IActionResult> Crear()
        {
            // Asegúrate de usar .ToList() para ejecutar la consulta inmediatamente
            var listaTipos = await _context.TiposMantenimiento.ToListAsync();
            ViewBag.Tipos = new SelectList(listaTipos ?? new List<TipoMantenimiento>(), "Id", "Nombre");
            ViewBag.IdVehiculo = new SelectList(await _context.Vehiculos.AsNoTracking().ToListAsync(), "Id", "Placa");
            return View();
        }

        // POST: Mantenimientos/Crear
        // Se asegura de incluir los nuevos campos en el Bind o simplemente recibir el modelo completo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Mantenimiento nuevoMantenimiento)
        {
            // --- ESTO ES LO QUE NECESITAS PARA DEPURAR ---
            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errores)
                {
                    // Si estás en modo Debug, pon un punto de interrupción aquí.
                    // O imprímelo en la ventana de "Salida" (Output) de Visual Studio
                    System.Diagnostics.Debug.WriteLine(error.ErrorMessage);
                }
            }
            // ----------------------------------------------

            if (ModelState.IsValid)
            {
                _context.Mantenimientos.Add(nuevoMantenimiento);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.IdVehiculo = new SelectList(await _context.Vehiculos.AsNoTracking().ToListAsync(), "Id", "Placa", nuevoMantenimiento.VehiculoId);
            return View(nuevoMantenimiento);
        }

        public async Task<IActionResult> Editar(int id)
        {
            var mantenimiento = await _context.Mantenimientos.FindAsync(id);
            if (mantenimiento == null) return NotFound();

            ViewBag.IdVehiculo = new SelectList(await _context.Vehiculos.AsNoTracking().ToListAsync(), "Id", "Placa", mantenimiento.VehiculoId);
            ViewBag.Tipos = new SelectList(_context.TiposMantenimiento, "Id", "Nombre", mantenimiento.TipoMantenimientoId);

            return View(mantenimiento);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Mantenimiento mantModificado, Mantenimiento mantenimiento)
        {
            if (id != mantModificado.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(mantModificado);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Manejo básico de error de concurrencia
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Tipos = new SelectList(_context.TiposMantenimiento, "Id", "Nombre", mantenimiento.TipoMantenimientoId);
            ViewBag.IdVehiculo = new SelectList(await _context.Vehiculos.AsNoTracking().ToListAsync(), "Id", "Placa", mantModificado.VehiculoId);
            return View(mantModificado);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var mantenimiento = await _context.Mantenimientos.FindAsync(id);
            if (mantenimiento != null)
            {
                _context.Mantenimientos.Remove(mantenimiento);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> GenerarReporteMantenimientoExcel(string placaFiltro, int? mes, int? anio)
        {
            // 1. Obtener placas a procesar (igual lógica)
            List<string> placasAProcesar = string.IsNullOrEmpty(placaFiltro)
                ? await _context.Mantenimientos.Select(m => m.IdVehiculoNavigation.Placa).Distinct().ToListAsync()
                : new List<string> { placaFiltro };

            var culture = new CultureInfo("es-ES");

            using (var package = new ExcelPackage())
            {
                foreach (var placa in placasAProcesar)
                {
                    var query = _context.Mantenimientos
     .Include(m => m.IdVehiculoNavigation)
     .Include(m => m.TipoMantenimiento) 
     .Where(m => m.IdVehiculoNavigation.Placa == placa);
                    if (mes.HasValue) query = query.Where(m => m.Fecha.Month == mes);
                    if (anio.HasValue && anio > 0) query = query.Where(m => m.Fecha.Year == anio);

                    var datosTaller = await query.OrderBy(m => m.Fecha).ToListAsync();

                    if (!datosTaller.Any()) continue;

                    string nombreHoja = placa.Length > 31 ? placa.Substring(0, 31) : placa;
                    var worksheet = package.Workbook.Worksheets.Add(nombreHoja);

                    // Encabezados específicos para Taller
                    string[] headers = { "Fecha", "Día Semana", "Placa", "Tipo Mantenimiento", "Descripción", "Kilometraje", "ProximoCambio", "Costo",  "Iva", "Garantia", "Taller" };

                    void EscribirEncabezados(int fila)
                    {
                        for (int i = 0; i < headers.Length; i++)
                        {
                            worksheet.Cells[fila, i + 1].Value = headers[i];
                            worksheet.Cells[fila, i + 1].Style.Font.Bold = true;
                        }
                    }

                    EscribirEncabezados(1);

                    int row = 2;
                    int currentMonth = 0;
                    decimal subCosto = 0;

                    foreach (var item in datosTaller)
                    {
                        if (currentMonth != 0 && item.Fecha.Month != currentMonth)
                        {
                            // Subtotal para Taller
                            worksheet.Cells[row, 5].Value = "SUBTOTAL MES:";
                            worksheet.Cells[row, 6].Value = subCosto;
                            worksheet.Cells[row, 5, row, 6].Style.Font.Bold = true;

                            row += 2; // Espacio
                            EscribirEncabezados(row);
                            row++;
                            subCosto = 0;
                        }

                        currentMonth = item.Fecha.Month;

                        worksheet.Cells[row, 1].Value = item.Fecha.ToShortDateString();
                        worksheet.Cells[row, 2].Value = item.Fecha.ToString("dddd", culture).ToUpper();
                        worksheet.Cells[row, 3].Value = item.IdVehiculoNavigation?.Placa;
                        worksheet.Cells[row, 4].Value = item.TipoMantenimiento?.Nombre;
                        worksheet.Cells[row, 5].Value = item.Descripcion;
                        worksheet.Cells[row, 6].Value = item.Kilometraje;
                        worksheet.Cells[row, 7].Value = item.ProximoCambio;
                        worksheet.Cells[row, 8].Value = item.Valor;                      
                        worksheet.Cells[row, 9].Value = item.Iva;
                        worksheet.Cells[row, 10].Value = item.Garantia;
                        worksheet.Cells[row, 11].Value = item.Taller;
                      
                        subCosto += item.Valor;
                        row++;
                    }

                    // Subtotal final
                    worksheet.Cells[row, 6].Value = "TOTAL MES:";
                    worksheet.Cells[row, 8].Value = subCosto;
                    worksheet.Cells[row, 5, row, 6].Style.Font.Bold = true;

                    worksheet.Cells.AutoFitColumns();
                }

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Reporte_Taller.xlsx");
            }
        }







    }
}