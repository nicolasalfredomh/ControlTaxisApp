using ControlTaxisApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Collections;
using System.Globalization;

namespace ControlTaxisApp.Controllers
{
    public class LiquidacionesController : Controller
    {
        private readonly ControlTaxisContext _context;

        public LiquidacionesController(ControlTaxisContext context)
        {
            _context = context;
           
        }

        // Método para verificar si el día no es laboral
        private async Task<bool> EsDiaNoLaboral(DateTime fecha)
        {
            // 1. Es domingo
            if (fecha.DayOfWeek == DayOfWeek.Sunday) return true;

            // 2. Es festivo (consulta la base de datos)
            return await _context.Festivos.AnyAsync<Festivo>(f => f.Fecha == fecha.Date);
        }

        // Método para verificar Pico y Placa
        public static bool EsPicoYPlaca(string placa, DateTime fecha)
        {
            if (string.IsNullOrEmpty(placa)) return false;
            char ultimoDigito = placa.LastOrDefault(char.IsDigit);
            if (ultimoDigito == default) return false;

            int dia = (int)fecha.DayOfWeek;
            if (dia == 1 && (ultimoDigito == '1' || ultimoDigito == '2')) return true;
            if (dia == 2 && (ultimoDigito == '3' || ultimoDigito == '4')) return true;
            if (dia == 3 && (ultimoDigito == '5' || ultimoDigito == '6')) return true;
            if (dia == 4 && (ultimoDigito == '7' || ultimoDigito == '8')) return true;
            if (dia == 5 && (ultimoDigito == '9' || ultimoDigito == '0')) return true;
            return false;
        }
        // GET: Liquidaciones
        public async Task<IActionResult> Index(string placaFiltro, int? mes, int? anio)
        {
            ViewBag.PlacasDisponibles = await _context.Vehiculos.Select(v => v.Placa).Distinct().OrderBy(p => p).ToListAsync();

            var query = _context.LiquidacionesDiarias.Include(l => l.Vehiculo).AsQueryable();

            if (!string.IsNullOrEmpty(placaFiltro))
                query = query.Where(l => l.Vehiculo != null && l.Vehiculo.Placa == placaFiltro);
            if (mes.HasValue)
                query = query.Where(l => l.Fecha.Month == mes);
            if (anio.HasValue && anio > 0)
                query = query.Where(l => l.Fecha.Year == anio);

            var lista = await query.OrderByDescending(l => l.Fecha).ToListAsync();

            // CARGA EFICIENTE DE FESTIVOS
            var fechasEnLista = lista.Select(l => l.Fecha.Date).Distinct().ToList();
            ViewBag.Festivos = await _context.Festivos
                .Where(f => fechasEnLista.Contains(f.Fecha))
                .Select(f => f.Fecha)
                .ToListAsync();

            return View(lista);
        }

        // GET: Crear
        public async Task<IActionResult> Crear()
        {
            // Cambiamos el nombre de la variable para que sea más claro
            var listaVehiculos = await _context.Vehiculos.ToListAsync();

            // Verificación de seguridad: si no hay vehículos, avisar
            if (listaVehiculos == null || !listaVehiculos.Any())
            {
                ViewBag.Error = "No hay vehículos registrados en el sistema.";
            }

            // Asegúrate que "Id" y "Placa" coincidan con los nombres exactos en tu modelo Vehiculo
            ViewBag.IdVehiculo = new SelectList(listaVehiculos, "Id", "Placa");

            return View();
        }

        // POST: Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(LiquidacionDiaria nuevaLiquidacion)
        {
            // 1. Calculamos el saldo en el servidor para asegurar precisión
            decimal gastos1 = nuevaLiquidacion.Gastos ?? 0; // Si es null, lo tratamos como 0
            decimal producido1 = nuevaLiquidacion.Producido; // Si es null, lo tratamos como 0
            nuevaLiquidacion.Saldo = producido1 - gastos1 ;

            // 2. Si la fecha llega vacía por error, le asignamos hoy
            if (nuevaLiquidacion.Fecha == DateTime.MinValue)
            {
                nuevaLiquidacion.Fecha = DateTime.Today;
            }

            if (ModelState.IsValid)
            {
                _context.LiquidacionesDiarias.Add(nuevaLiquidacion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Si hay error, recargamos la lista de vehículos
            ViewBag.IdVehiculo = new SelectList(await _context.Vehiculos.ToListAsync(), "Id", "Placa", nuevaLiquidacion.VehiculoId);
            return View(nuevaLiquidacion);
        }

        // GET: Editar
        // GET: Editar
        public async Task<IActionResult> Editar(int id)
        {
            var liquidacion = await _context.LiquidacionesDiarias.FindAsync(id);
            if (liquidacion == null) return NotFound();

            ViewBag.IdVehiculo = new SelectList(await _context.Vehiculos.ToListAsync(), "Id", "Placa", liquidacion.VehiculoId);

            // AQUÍ ESTÁ EL SECRETO: Debes enviar el objeto a la vista
            return View(liquidacion);
        }

        // POST: Editar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, LiquidacionDiaria liquidacionModificada)
        {
            if (id != liquidacionModificada.Id) return NotFound();

            // 1. Obtener la entidad original de la base de datos
            var liquidacionOriginal = await _context.LiquidacionesDiarias.FindAsync(id);

            if (liquidacionOriginal == null) return NotFound();

            // 2. Actualizar solo los campos que vienen del formulario
            liquidacionOriginal.VehiculoId = liquidacionModificada.VehiculoId;
            liquidacionOriginal.Producido = liquidacionModificada.Producido;
            liquidacionOriginal.Gastos = liquidacionModificada.Gastos;
            liquidacionOriginal.Ahorro = liquidacionModificada.Ahorro;
            liquidacionOriginal.PicoYPlaca = liquidacionModificada.PicoYPlaca;
            // 3. Calcular el saldo manualmente
            decimal gastos = liquidacionModificada.Gastos ?? 0; // Si es null, lo tratamos como 0
            decimal producido = liquidacionModificada.Producido; // Si es null, lo tratamos como 0
            liquidacionOriginal.Saldo = producido - gastos ;

            // Nota: NO tocamos 'liquidacionOriginal.Fecha', por lo tanto, se mantiene intacta.

            if (ModelState.IsValid)
            {
               
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(liquidacionModificada);
        }
        // POST: Eliminar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var liquidacion = await _context.LiquidacionesDiarias.FindAsync(id);
            if (liquidacion != null)
            {
                _context.LiquidacionesDiarias.Remove(liquidacion);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> GenerarReporteExcel(string placaFiltro, int? mes, int? anio)
        {
            List<string> placasAProcesar = string.IsNullOrEmpty(placaFiltro)
                ? await _context.LiquidacionesDiarias.Select(l => l.Vehiculo.Placa).Distinct().ToListAsync()
                : new List<string> { placaFiltro };

            var festivos = await _context.Festivos.Select(f => f.Fecha.Date).ToListAsync();
            var culture = new CultureInfo("es-ES");

            using (var package = new ExcelPackage())
            {
                foreach (var placa in placasAProcesar)
                {
                    var query = _context.LiquidacionesDiarias.Include(l => l.Vehiculo).Where(l => l.Vehiculo.Placa == placa);
                    if (mes.HasValue) query = query.Where(l => l.Fecha.Month == mes);
                    if (anio.HasValue && anio > 0) query = query.Where(l => l.Fecha.Year == anio);

                    var datosPlaca = await query.OrderBy(l => l.Fecha).ToListAsync();

                    if (!datosPlaca.Any()) continue;

                    string nombreHoja = placa.Length > 31 ? placa.Substring(0, 31) : placa;
                    var worksheet = package.Workbook.Worksheets.Add(nombreHoja);

                    // Definimos los encabezados en una lista para reutilizarlos
                    string[] headers = { "Fecha", "Día Semana", "Placa", "Producido", "Gastos", "Ahorro", "Saldo", "Día Laboral", "Pico y Placa" };

                    // Método local para escribir encabezados
                    void EscribirEncabezados(int fila)
                    {
                        for (int i = 0; i < headers.Length; i++)
                        {
                            worksheet.Cells[fila, i + 1].Value = headers[i];
                            worksheet.Cells[fila, i + 1].Style.Font.Bold = true; // Negrita para destacar
                        }
                    }

                    EscribirEncabezados(1); // Encabezado inicial

                    int row = 2;
                    int currentMonth = 0;
                    decimal subProducido = 0, subGastos = 0, subAhorro = 0, subSaldo = 0;

                    foreach (var item in datosPlaca)
                    {
                        // Si cambia el mes, insertamos subtotal y REPETIMOS encabezados
                        if (currentMonth != 0 && item.Fecha.Month != currentMonth)
                        {
                            AgregarFilaSubtotal(worksheet, ref row, subProducido, subGastos, subAhorro, subSaldo);
                            row++; // Espacio en blanco antes de los nuevos títulos
                            EscribirEncabezados(row); // <--- REPETICIÓN DE TÍTULOS
                            row++;

                            subProducido = subGastos = subAhorro = subSaldo = 0;
                        }

                        currentMonth = item.Fecha.Month;
                        // ... (resto de tu lógica de datos sigue igual)

                        worksheet.Cells[row, 1].Value = item.Fecha.ToShortDateString();
                        worksheet.Cells[row, 2].Value = item.Fecha.ToString("dddd", culture).ToUpper();
                        worksheet.Cells[row, 3].Value = item.Vehiculo?.Placa;
                        worksheet.Cells[row, 4].Value = item.Producido;
                        worksheet.Cells[row, 5].Value = item.Gastos;
                        worksheet.Cells[row, 6].Value = item.Ahorro;
                        worksheet.Cells[row, 7].Value = item.Saldo;
                        worksheet.Cells[row, 8].Value = (item.Fecha.DayOfWeek == DayOfWeek.Sunday ? "DOMINGO" : (festivos.Contains(item.Fecha.Date) ? "FESTIVO" : "HÁBIL"));
                        worksheet.Cells[row, 9].Value = (LiquidacionesController.EsPicoYPlaca(item.Vehiculo?.Placa, item.Fecha) ? "SÍ" : "NO");

                      
                        subProducido += item.Producido;
                        subGastos +=item.Gastos ?? 0;
                        subAhorro += item.Ahorro ?? 0;
                        subSaldo += item.Saldo;
                        row++;
                    }
                    AgregarFilaSubtotal(worksheet, ref row, subProducido, subGastos, subAhorro, subSaldo);
                    worksheet.Cells.AutoFitColumns();
                }

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Reporte_Mensualizado_Detallado.xlsx");
            }
        }

        private void AgregarFilaSubtotal(ExcelWorksheet ws, ref int row, decimal prod, decimal gast, decimal ahor, decimal sald)
        {
            ws.Cells[row, 2].Value = "SUBTOTAL MES";
            ws.Cells[row, 2].Style.Font.Bold = true;
            ws.Cells[row, 4].Value = prod;
            ws.Cells[row, 5].Value = gast;
            ws.Cells[row, 6].Value = ahor;
            ws.Cells[row, 7].Value = sald;
            ws.Cells[row, 4, row, 7].Style.Font.Bold = true;
        }


    }
}