using ControlTaxisApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Collections;
using System.Globalization;
using System.Security.Claims;

namespace ControlTaxisApp.Controllers
{
    public class LiquidacionesController : Controller
    {

        private readonly ControlTaxisContext _context;
        private readonly string _userId;
       
        public LiquidacionesController(ControlTaxisContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userId = httpContextAccessor.HttpContext?.User.Identity?.Name;
            Console.WriteLine($"DEBUG: Usuario logueado detectado como: {_userId}");
            var meses = new[]
            {
    new { Valor = 1, Nombre = "Enero" },
    new { Valor = 2, Nombre = "Febrero" },
    new { Valor = 3, Nombre = "Marzo" },
    new { Valor = 4, Nombre = "Abril" },
    new { Valor = 5, Nombre = "Mayo" },
    new { Valor = 6, Nombre = "Junio" },
    new { Valor = 7, Nombre = "Julio" },
    new { Valor = 8, Nombre = "Agosto" },
    new { Valor = 9, Nombre = "Septiembre" },
    new { Valor = 10, Nombre = "Octubre" },
    new { Valor = 11, Nombre = "Noviembre" },
    new { Valor = 12, Nombre = "Diciembre" }
};

            ViewBag.Meses = meses;

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


        private async Task<bool> EsPropietario(int vehiculoId)
        {
            return await _context.Vehiculos
                                 .AnyAsync(v => v.Id == vehiculoId && v.UsuarioId == _userId);
        }
        // GET: Liquidaciones
        public async Task<IActionResult> Index(string placaFiltro, int? mes, int? anio)
        {


            var placasUsuario = await _context.Vehiculos.Where(v => v.UsuarioId == _userId).ToListAsync();
            ViewBag.PlacasDisponibles = placasUsuario.Select(v => v.Placa).ToList();
            Console.WriteLine($"Usuario logueado: {_userId}");
            // Filtramos liquidaciones donde el vehículo pertenezca al usuario
            var query = _context.LiquidacionesDiarias
                                .Include(l => l.Vehiculo)
                                .Where(l => l.Vehiculo != null && l.Vehiculo.UsuarioId == _userId)
                                .AsQueryable();

           
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
            var vehiculos = await _context.Vehiculos.Where(v => v.UsuarioId == _userId).ToListAsync();
          
            // Verificación de seguridad: si no hay vehículos, avisar
            if (vehiculos == null || !vehiculos.Any())
            {
                ViewBag.Error = "No hay vehículos registrados en el sistema.";
            }

            // Asegúrate que "Id" y "Placa" coincidan con los nombres exactos en tu modelo Vehiculo
            ViewBag.IdVehiculo = new SelectList(vehiculos, "Id", "Placa");

            return View();
        }

        // POST: Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(LiquidacionDiaria nuevaLiquidacion)
        {
            // Validamos propiedad
            if (!await EsPropietario(nuevaLiquidacion.VehiculoId)) return Forbid();

            nuevaLiquidacion.Saldo = nuevaLiquidacion.Producido - (nuevaLiquidacion.Gastos ?? 0);

            if (nuevaLiquidacion.Fecha == DateTime.MinValue)
                nuevaLiquidacion.Fecha = DateTime.Today;

            if (ModelState.IsValid)
            {
                _context.LiquidacionesDiarias.Add(nuevaLiquidacion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // CORRECCIÓN: Filtramos por _userId al recargar la lista
            var vehiculos = await _context.Vehiculos.Where(v => v.UsuarioId == _userId).ToListAsync();
            ViewBag.IdVehiculo = new SelectList(vehiculos, "Id", "Placa", nuevaLiquidacion.VehiculoId);
            return View(nuevaLiquidacion);
        }

        // GET: Editar

        public async Task<IActionResult> Editar(int id)
        {
            var liquidacion = await _context.LiquidacionesDiarias.Include(l => l.Vehiculo).FirstOrDefaultAsync(l => l.Id == id);
            if (liquidacion == null || liquidacion.Vehiculo.UsuarioId != _userId) return NotFound();

            ViewBag.IdVehiculo = new SelectList(await _context.Vehiculos.Where(v => v.UsuarioId == _userId).ToListAsync(), "Id", "Placa", liquidacion.VehiculoId);
            return View(liquidacion);
        }

        // POST: Editar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, LiquidacionDiaria liquidacionModificada)
        {
            if (id != liquidacionModificada.Id) return NotFound();

            var liquidacionOriginal = await _context.LiquidacionesDiarias
                .Include(l => l.Vehiculo)
                .FirstOrDefaultAsync(l => l.Id == id);

            // Validamos que exista y que pertenezca al usuario
            if (liquidacionOriginal == null || liquidacionOriginal.Vehiculo.UsuarioId != _userId)
                return NotFound();

            // Validamos que el vehículo nuevo seleccionado también sea del usuario
            if (!await EsPropietario(liquidacionModificada.VehiculoId)) return Forbid();

            liquidacionOriginal.VehiculoId = liquidacionModificada.VehiculoId;
            liquidacionOriginal.Producido = liquidacionModificada.Producido;
            liquidacionOriginal.Gastos = liquidacionModificada.Gastos;
            liquidacionOriginal.Saldo = liquidacionModificada.Producido - (liquidacionModificada.Gastos ?? 0);

            if (ModelState.IsValid)
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // CORRECCIÓN: Filtramos por _userId al recargar la lista
            var vehiculos = await _context.Vehiculos.Where(v => v.UsuarioId == _userId).ToListAsync();
            ViewBag.IdVehiculo = new SelectList(vehiculos, "Id", "Placa", liquidacionModificada.VehiculoId);
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



        [HttpPost]
        public async Task<IActionResult> ImportarLiquidaciones(IFormFile archivoExcel)
        {
            if (archivoExcel == null || archivoExcel.Length == 0)
                return Content("Error: No se ha seleccionado ningún archivo.");

            try
            {
                using (var package = new ExcelPackage(archivoExcel.OpenReadStream()))
                {
                    // Recorremos TODAS las hojas del archivo
                    foreach (var worksheet in package.Workbook.Worksheets)
                    {
                        string nombreHoja = worksheet.Name.Trim().ToUpper();

                        // Buscamos el vehículo específico para esta hoja
                        var vehiculo = _context.Vehiculos.FirstOrDefault(v => v.Placa.ToUpper() == nombreHoja);

                        if (vehiculo == null) continue; // Si no existe la placa, saltamos esta hoja

                        int rowCount = worksheet.Dimension.Rows;

                        for (int row = 3; row <= rowCount; row++) // Tus datos empiezan en fila 3 según la imagen
                        {
                            var celdaFecha = worksheet.Cells[row, 1].Value?.ToString();

                            // Saltamos encabezados o filas vacías
                            if (string.IsNullOrEmpty(celdaFecha) || celdaFecha.Trim().ToUpper() == "FECHA") continue;

                            if (!DateTime.TryParse(celdaFecha, out DateTime fecha)) continue;

                            var estadoRaw = worksheet.Cells[row, 3].Value?.ToString()?.ToUpper() ?? "NORMAL";

                            // Limpiamos los valores decimales (eliminando puntos de miles si existen)
                            var producido = ParseDecimal(worksheet.Cells[row, 4].Value);
                            var gastos = ParseDecimal(worksheet.Cells[row, 5].Value);
                            var ahorro = ParseDecimal(worksheet.Cells[row, 6].Value);

                            string estadoFinal = estadoRaw switch
                            {
                                "PICO Y PLACA" => "PICO_Y_PLACA",
                                "TALLER" => "TALLER",
                                _ => "NORMAL"
                            };

                            var liquidacion = new LiquidacionDiaria
                            {
                                VehiculoId = vehiculo.Id,
                                Fecha = fecha,
                                Producido = producido,
                                Gastos = gastos,
                                Ahorro = ahorro,
                                Saldo = producido - gastos ,
                                EstadoDia = estadoFinal
                            };

                            _context.LiquidacionesDiarias.Add(liquidacion);
                        }
                    }
                    await _context.SaveChangesAsync();
                   
                }
            }
            catch (Exception ex)
            {
                return Content($"Error crítico: {ex.Message}");
            }
        }

        // Método auxiliar para limpiar números como "95.000"
        private decimal ParseDecimal(object value)
        {
            if (value == null) return 0;
            string str = value.ToString().Replace(".", "").Replace(",", ".");
            return decimal.TryParse(str, out decimal result) ? result : 0;
        }


    }
}