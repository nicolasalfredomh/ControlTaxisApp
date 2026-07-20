using ControlTaxisApp.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ControlTaxisApp.Services
{
    public class ReporteService
    {
        public byte[] GenerarExcel(List<LiquidacionDiaria> liquidaciones, List<Mantenimiento> mantenimientos, List<DateTime> festivos, List<GastoAdministrativo> gastosAdmin)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();

            // --- HOJA DE LIQUIDACIONES ---
            var placas = liquidaciones.GroupBy(l => l.Vehiculo?.Placa ?? "Sin Placa");

            foreach (var placaGroup in placas)
            {
                var ws = package.Workbook.Worksheets.Add(placaGroup.Key);
                var meses = placaGroup.GroupBy(l => l.Fecha.Month);
                int filaActual = 1;

                foreach (var mesGroup in meses)
                {
                    // Nombre del mes en español
                    string nombreMes = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(mesGroup.Key).ToUpper();
                    ws.Cells[filaActual, 1].Value = "MES: " + nombreMes;
                    ws.Cells[filaActual, 1, filaActual, 7].Merge = true;
                    ws.Cells[filaActual, 1].Style.Font.Bold = true;
                    filaActual++;

                    // Cabeceras ajustadas
                    string[] headers = { "Fecha", "Día", "Producido", "Gastos", "Ahorro", "Saldo", "Tipo Día", "Estado" };
                    for (int i = 0; i < headers.Length; i++) ws.Cells[filaActual, i + 1].Value = headers[i];
                    ws.Cells[filaActual, 1, filaActual, 8].Style.Font.Bold = true;
                    filaActual++;

                    foreach (var item in mesGroup)
                    {
                        string diaSemana = CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(item.Fecha.DayOfWeek);
                        string tipoDia = item.Fecha.DayOfWeek == DayOfWeek.Sunday ? "DOMINGO" :
                                         festivos.Contains(item.Fecha.Date) ? "FESTIVO" : "HÁBIL";

                        ws.Cells[filaActual, 1].Value = item.Fecha.ToString("dd/MM/yyyy");
                        ws.Cells[filaActual, 2].Value = char.ToUpper(diaSemana[0]) + diaSemana.Substring(1);
                        ws.Cells[filaActual, 3].Value = item.Producido;
                        ws.Cells[filaActual, 4].Value = item.Gastos;
                        ws.Cells[filaActual, 5].Value = item.Ahorro;
                        ws.Cells[filaActual, 6].Value = item.Saldo;
                        ws.Cells[filaActual, 7].Value = tipoDia;
                        ws.Cells[filaActual, 8].Value = item.EstadoDia;
                        filaActual++;
                    }

                    // Totales del mes
                    ws.Cells[filaActual, 2].Value = "TOTAL MES";
                    ws.Cells[filaActual, 3].Formula = $"SUM(C{filaActual - mesGroup.Count()}:C{filaActual - 1})";
                    ws.Cells[filaActual, 4].Formula = $"SUM(D{filaActual - mesGroup.Count()}:D{filaActual - 1})";
                    ws.Cells[filaActual, 6].Formula = $"SUM(F{filaActual - mesGroup.Count()}:F{filaActual - 1})";
                    ws.Cells[filaActual, 2, filaActual, 6].Style.Font.Bold = true;
                    filaActual += 2;
                }
            }

            // 2. --- MANTENIMIENTOS (Hoja por placa, meses uno debajo del otro) ---
            var mantPorPlaca = mantenimientos.GroupBy(m => m.IdVehiculoNavigation?.Placa ?? "Sin Placa");
            foreach (var grupoPlaca in mantPorPlaca)
            {
                var wsM = package.Workbook.Worksheets.Add($"Mant_{grupoPlaca.Key}");
                int filaActual = 1;

                var meses = grupoPlaca.GroupBy(m => m.Fecha.Month);
                foreach (var mesGroup in meses)
                {
                    string nombreMes = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(mesGroup.Key).ToUpper();
                    wsM.Cells[filaActual, 1].Value = "MES: " + nombreMes;
                    wsM.Cells[filaActual, 1, filaActual, 5].Merge = true;
                    wsM.Cells[filaActual, 1].Style.Font.Bold = true;
                    filaActual++;

                    string[] headers = { "Fecha", "Placa", "Tipo", "Descripción", "Costo" };
                    for (int i = 0; i < headers.Length; i++) wsM.Cells[filaActual, i + 1].Value = headers[i];
                    wsM.Cells[filaActual, 1, filaActual, 5].Style.Font.Bold = true;
                    filaActual++;

                    foreach (var item in mesGroup)
                    {
                        wsM.Cells[filaActual, 1].Value = item.Fecha.ToString("dd/MM/yyyy");
                        wsM.Cells[filaActual, 2].Value = grupoPlaca.Key;
                        wsM.Cells[filaActual, 3].Value = item.TipoMantenimiento?.Nombre ?? "N/A";
                        wsM.Cells[filaActual, 4].Value = item.Descripcion;
                        wsM.Cells[filaActual, 5].Value = item.Valor;
                        filaActual++;
                    }
                    filaActual++; // Espacio entre meses
                }
            }

            // 3. --- GASTOS ADMINISTRATIVOS (Hoja por placa, meses uno debajo del otro) ---
            var gastosPorPlaca = gastosAdmin.GroupBy(g => g.Placa ?? "Sin Placa");
            foreach (var grupoPlaca in gastosPorPlaca)
            {
                var wsG = package.Workbook.Worksheets.Add($"Gastos_{grupoPlaca.Key}");
                int filaActual = 1;

                var meses = grupoPlaca.GroupBy(g => g.Fecha.Month);
                foreach (var mesGroup in meses)
                {
                    string nombreMes = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(mesGroup.Key).ToUpper();
                    wsG.Cells[filaActual, 1].Value = "MES: " + nombreMes;
                    wsG.Cells[filaActual, 1, filaActual, 5].Merge = true;
                    wsG.Cells[filaActual, 1].Style.Font.Bold = true;
                    filaActual++;

                    string[] headers = { "Fecha", "Placa", "Concepto", "Valor", "Observaciones" };
                    for (int i = 0; i < headers.Length; i++) wsG.Cells[filaActual, i + 1].Value = headers[i];
                    wsG.Cells[filaActual, 1, filaActual, 5].Style.Font.Bold = true;
                    filaActual++;

                    foreach (var item in mesGroup)
                    {
                        wsG.Cells[filaActual, 1].Value = item.Fecha.ToString("dd/MM/yyyy");
                        wsG.Cells[filaActual, 2].Value = grupoPlaca.Key;
                        wsG.Cells[filaActual, 3].Value = item.Concepto;
                        wsG.Cells[filaActual, 4].Value = item.Valor;
                        wsG.Cells[filaActual, 5].Value = item.Observaciones;
                        filaActual++;
                    }
                    filaActual++; // Espacio entre meses
                }
            }

            return package.GetAsByteArray();
        }
    }
}