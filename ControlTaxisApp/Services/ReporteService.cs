using ControlTaxisApp.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
namespace ControlTaxisApp.Services

{
    public class ReporteService
    {
        public byte[] GenerarExcel(List<LiquidacionDiaria> liquidaciones, List<Mantenimiento> mantenimientos)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();

            // Agrupamos por Placa
            var placas = liquidaciones.GroupBy(l => l.Vehiculo?.Placa ?? "Sin Placa");

            foreach (var placaGroup in placas)
            {
                var ws = package.Workbook.Worksheets.Add(placaGroup.Key); // Una hoja por placa

                // Agrupamos por mes dentro de la hoja
                var meses = placaGroup.GroupBy(l => l.Fecha.Month);
                int filaActual = 1;

                foreach (var mesGroup in meses)
                {
                    ws.Cells[filaActual, 1].Value = "Mes: " + mesGroup.Key;
                    ws.Cells[filaActual, 1, filaActual, 6].Merge = true;
                    ws.Cells[filaActual, 1].Style.Font.Bold = true;
                    filaActual++;

                    // Cabeceras
                    string[] headers = { "Fecha", "Producido", "Gastos", "Ahorro", "Saldo", "Estado" };
                    for (int i = 0; i < headers.Length; i++) ws.Cells[filaActual, i + 1].Value = headers[i];
                    filaActual++;

                    foreach (var item in mesGroup)
                    {
                        ws.Cells[filaActual, 1].Value = item.Fecha.ToShortDateString();
                        ws.Cells[filaActual, 2].Value = item.Producido;
                        ws.Cells[filaActual, 3].Value = item.Gastos;
                        ws.Cells[filaActual, 4].Value = item.Ahorro;
                        ws.Cells[filaActual, 5].Value = item.Saldo;
                        ws.Cells[filaActual, 6].Value = item.EstadoDia;
                        filaActual++;
                    }

                    // Totales del mes
                    ws.Cells[filaActual, 1].Value = "TOTAL MES";
                    ws.Cells[filaActual, 2].Formula = $"SUM(B{filaActual - mesGroup.Count()}:B{filaActual - 1})";
                    ws.Cells[filaActual, 3].Formula = $"SUM(C{filaActual - mesGroup.Count()}:C{filaActual - 1})";
                    filaActual += 2;
                }
            }


            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
         
            var wsM = package.Workbook.Worksheets.Add("Mantenimientos");
            wsM.Cells[1, 1].Value = "Fecha";
            wsM.Cells[1, 2].Value = "Placa";
            wsM.Cells[1, 3].Value = "Descripción";
            wsM.Cells[1, 4].Value = "Costo";
            wsM.Cells[1, 1, 1, 4].Style.Font.Bold = true;

            int fila = 2;
            foreach (var item in mantenimientos)
            {
                wsM.Cells[fila, 1].Value = item.Fecha.ToShortDateString();
                wsM.Cells[fila, 2].Value = item.IdVehiculoNavigation?.Placa;
                wsM.Cells[fila, 3].Value = item.Descripcion;
                wsM.Cells[fila, 4].Value = item.Valor;
                fila++;
            }

            // Total de gastos de mantenimiento
            wsM.Cells[fila, 3].Value = "TOTAL GASTOS";
            wsM.Cells[fila, 4].Formula = $"SUM(D2:D{fila - 1})";
            wsM.Cells[fila, 3, fila, 4].Style.Font.Bold = true;



            return package.GetAsByteArray();
        }
    }

}
