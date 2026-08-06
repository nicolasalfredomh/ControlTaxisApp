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
                ws.View.ShowGridLines = true;
                var meses = placaGroup.GroupBy(l => l.Fecha.Month);
                int filaActual = 1;

                foreach (var mesGroup in meses)
                {
                    int anio = mesGroup.First().Fecha.Year;
                    string nombreMes = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(mesGroup.Key).ToUpper();

                    // 1. TÍTULO PRINCIPAL (Centrado, con Placa, fondo azul y texto blanco)
                    ws.Cells[filaActual, 1, filaActual, 8].Merge = true;
                    var celdaTitulo = ws.Cells[filaActual, 1];
                    celdaTitulo.Value = $"{nombreMes} {anio} {placaGroup.Key}";
                    celdaTitulo.Style.Font.Bold = true;
                    celdaTitulo.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    celdaTitulo.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    celdaTitulo.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(31, 78, 121)); // Azul oscuro
                    celdaTitulo.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    celdaTitulo.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Row(filaActual).Height = 25;
                    filaActual++;

                    // 2. Cabeceras ajustadas con bordes negros
                    string[] headers = { "Fecha", "Día", "Producido", "Gastos", "Ahorro", "Saldo", "Tipo Día", "Estado" };
                    for (int i = 0; i < headers.Length; i++)
                    {
                        var cell = ws.Cells[filaActual, i + 1];
                        cell.Value = headers[i];
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(220, 225, 230));
                        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }

                    var rangoCabecera = ws.Cells[filaActual, 1, filaActual, 8];
                    rangoCabecera.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    rangoCabecera.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    rangoCabecera.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    rangoCabecera.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    rangoCabecera.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                    rangoCabecera.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                    rangoCabecera.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                    rangoCabecera.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                    filaActual++;

                    int inicioDatos = filaActual;

                    foreach (var item in mesGroup)
                    {
                        string diaSemana = CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(item.Fecha.DayOfWeek);
                        bool esDomingo = item.Fecha.DayOfWeek == DayOfWeek.Sunday;
                        bool esFestivo = festivos.Contains(item.Fecha.Date);
                        bool esPicoPlaca = item.EstadoDia == "PICO_Y_PLACA";

                        ws.Cells[filaActual, 1].Value = item.Fecha.ToString("dd/MM/yyyy");
                        ws.Cells[filaActual, 2].Value = char.ToUpper(diaSemana[0]) + diaSemana.Substring(1);
                        ws.Cells[filaActual, 3].Value = item.Producido;
                        ws.Cells[filaActual, 4].Value = item.Gastos;
                        ws.Cells[filaActual, 5].Value = item.Ahorro;
                        ws.Cells[filaActual, 6].Value = item.Saldo;
                        ws.Cells[filaActual, 7].Value = esDomingo ? "DOMINGO" : (esFestivo ? "FESTIVO" : (esPicoPlaca ? "PICO Y PLACA" : "HÁBIL"));
                        ws.Cells[filaActual, 8].Value = item.EstadoDia;

                        var rangoFila = ws.Cells[filaActual, 1, filaActual, 8];

                        // Aplicar bordes negros a cada celda de la fila de datos
                        for (int col = 1; col <= 8; col++)
                        {
                            var c = ws.Cells[filaActual, col];
                            c.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            c.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            c.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            c.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            c.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                            c.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                            c.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                            c.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                        }

                        if (esDomingo || esFestivo)
                        {
                            rangoFila.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            rangoFila.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Red);
                            rangoFila.Style.Font.Color.SetColor(System.Drawing.Color.White);
                        }
                        else if (esPicoPlaca)
                        {
                            rangoFila.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            rangoFila.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                        }

                        filaActual++;
                    }

                    // 3. Totales del mes con bordes negros
                    ws.Cells[filaActual, 2].Value = "TOTAL MES";
                    ws.Cells[filaActual, 3].Formula = $"SUM(C{inicioDatos}:C{filaActual - 1})";
                    ws.Cells[filaActual, 4].Formula = $"SUM(D{inicioDatos}:D{filaActual - 1})";
                    ws.Cells[filaActual, 6].Formula = $"SUM(F{inicioDatos}:F{filaActual - 1})";
                    ws.Cells[filaActual, 2, filaActual, 6].Style.Font.Bold = true;

                    for (int col = 1; col <= 8; col++)
                    {
                        var c = ws.Cells[filaActual, col];
                        c.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        c.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        c.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        c.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        c.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                        c.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                        c.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                        c.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                    }

                    filaActual += 2;
                }

                ws.Cells.AutoFitColumns();
            }

            // --- MANTENIMIENTOS ---
            var mantPorPlaca = mantenimientos.GroupBy(m => m.IdVehiculoNavigation?.Placa ?? "Sin Placa");
            foreach (var grupoPlaca in mantPorPlaca)
            {
                var wsM = package.Workbook.Worksheets.Add($"Mant_{grupoPlaca.Key}");
                wsM.View.ShowGridLines = true;
                int filaActual = 1;

                var meses = grupoPlaca.GroupBy(m => m.Fecha.Month);
                foreach (var mesGroup in meses)
                {
                    int anio = mesGroup.First().Fecha.Year;
                    string nombreMes = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(mesGroup.Key).ToUpper();

                    // Título principal centrado con placa y fondo azul
                    wsM.Cells[filaActual, 1, filaActual, 5].Merge = true;
                    var celdaTitulo = wsM.Cells[filaActual, 1];
                    celdaTitulo.Value = $"{nombreMes} {anio} {grupoPlaca.Key}";
                    celdaTitulo.Style.Font.Bold = true;
                    celdaTitulo.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    celdaTitulo.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    celdaTitulo.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(31, 78, 121));
                    celdaTitulo.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    celdaTitulo.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    wsM.Row(filaActual).Height = 25;
                    filaActual++;

                    string[] headers = { "Fecha", "Placa", "Tipo", "Descripción", "Costo" };
                    for (int i = 0; i < headers.Length; i++)
                    {
                        var cell = wsM.Cells[filaActual, i + 1];
                        cell.Value = headers[i];
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(220, 225, 230));
                        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }

                    var rangoCabecera = wsM.Cells[filaActual, 1, filaActual, 5];
                    rangoCabecera.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    rangoCabecera.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    rangoCabecera.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    rangoCabecera.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    rangoCabecera.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                    rangoCabecera.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                    rangoCabecera.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                    rangoCabecera.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                    filaActual++;

                    foreach (var item in mesGroup)
                    {
                        wsM.Cells[filaActual, 1].Value = item.Fecha.ToString("dd/MM/yyyy");
                        wsM.Cells[filaActual, 2].Value = grupoPlaca.Key;
                        wsM.Cells[filaActual, 3].Value = item.TipoMantenimiento?.Nombre ?? "N/A";
                        wsM.Cells[filaActual, 4].Value = item.Descripcion;
                        wsM.Cells[filaActual, 5].Value = item.Valor;

                        for (int col = 1; col <= 5; col++)
                        {
                            var c = wsM.Cells[filaActual, col];
                            c.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            c.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            c.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            c.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            c.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                            c.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                            c.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                            c.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                        }
                        filaActual++;
                    }
                    filaActual++;
                }
                wsM.Cells.AutoFitColumns();
            }

            // --- GASTOS ADMINISTRATIVOS ---
            var gastosPorPlaca = gastosAdmin.GroupBy(g => g.Placa ?? "Sin Placa");
            foreach (var grupoPlaca in gastosPorPlaca)
            {
                var wsG = package.Workbook.Worksheets.Add($"Gastos_{grupoPlaca.Key}");
                wsG.View.ShowGridLines = true;
                int filaActual = 1;

                var meses = grupoPlaca.GroupBy(g => g.Fecha.Month);
                foreach (var mesGroup in meses)
                {
                    int anio = mesGroup.First().Fecha.Year;
                    string nombreMes = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(mesGroup.Key).ToUpper();

                    // Título principal centrado con placa y fondo azul
                    wsG.Cells[filaActual, 1, filaActual, 5].Merge = true;
                    var celdaTitulo = wsG.Cells[filaActual, 1];
                    celdaTitulo.Value = $"{nombreMes} {anio} {grupoPlaca.Key}";
                    celdaTitulo.Style.Font.Bold = true;
                    celdaTitulo.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    celdaTitulo.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    celdaTitulo.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(31, 78, 121));
                    celdaTitulo.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    celdaTitulo.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    wsG.Row(filaActual).Height = 25;
                    filaActual++;

                    string[] headers = { "Fecha", "Placa", "Concepto", "Valor", "Observaciones" };
                    for (int i = 0; i < headers.Length; i++)
                    {
                        var cell = wsG.Cells[filaActual, i + 1];
                        cell.Value = headers[i];
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(220, 225, 230));
                        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }

                    var rangoCabecera = wsG.Cells[filaActual, 1, filaActual, 5];
                    rangoCabecera.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    rangoCabecera.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    rangoCabecera.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    rangoCabecera.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    rangoCabecera.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                    rangoCabecera.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                    rangoCabecera.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                    rangoCabecera.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                    filaActual++;

                    foreach (var item in mesGroup)
                    {
                        wsG.Cells[filaActual, 1].Value = item.Fecha.ToString("dd/MM/yyyy");
                        wsG.Cells[filaActual, 2].Value = grupoPlaca.Key;
                        wsG.Cells[filaActual, 3].Value = item.Concepto;
                        wsG.Cells[filaActual, 4].Value = item.Valor;
                        wsG.Cells[filaActual, 5].Value = item.Observaciones;

                        for (int col = 1; col <= 5; col++)
                        {
                            var c = wsG.Cells[filaActual, col];
                            c.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            c.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            c.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            c.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            c.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                            c.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                            c.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                            c.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                        }
                        filaActual++;
                    }
                    filaActual++;
                }
                wsG.Cells.AutoFitColumns();
            }

            return package.GetAsByteArray();
        }
    }
}