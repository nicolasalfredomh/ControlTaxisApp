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
        public byte[] GenerarExcel(List<LiquidacionDiaria> liquidaciones, List<Mantenimiento> mantenimientos, List<DateTime> festivos)
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

            // --- HOJA DE MANTENIMIENTOS ---
            var wsM = package.Workbook.Worksheets.Add("Mantenimientos");
            string[] headersM = { "Fecha", "Placa", "Tipo Mantenimiento", "Descripción", "Costo" };
            for (int i = 0; i < headersM.Length; i++) wsM.Cells[1, i + 1].Value = headersM[i];
            wsM.Cells[1, 1, 1, 5].Style.Font.Bold = true;

            int fila = 2;
            foreach (var item in mantenimientos)
            {
                wsM.Cells[fila, 1].Value = item.Fecha.ToString("dd/MM/yyyy");
                wsM.Cells[fila, 2].Value = item.IdVehiculoNavigation?.Placa;
                wsM.Cells[fila, 3].Value = item.TipoMantenimiento?.Nombre ?? "N/A";
                wsM.Cells[fila, 4].Value = item.Descripcion;
                wsM.Cells[fila, 5].Value = item.Valor;
                fila++;
            }

            wsM.Cells[fila, 4].Value = "TOTAL GASTOS";
            wsM.Cells[fila, 5].Formula = $"SUM(E2:E{fila - 1})";
            wsM.Cells[fila, 4, fila, 5].Style.Font.Bold = true;

            return package.GetAsByteArray();
        }
    }
}