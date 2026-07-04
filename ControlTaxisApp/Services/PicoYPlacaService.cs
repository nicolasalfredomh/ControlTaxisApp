using System;

namespace ControlTaxisApp.Services
{
    public static class PicoYPlacaService
    {
        public static bool TienePicoYPlaca(string placa, DateTime fecha)
        {
            // Los domingos no hay pico y placa
            if (fecha.DayOfWeek == DayOfWeek.Sunday) return false;

            // Obtener el último dígito de la placa
            string digitoStr = placa.Substring(placa.Length - 1);
            if (!int.TryParse(digitoStr, out int ultimoDigito)) return false;

            // Lógica Bogotá: Días pares (1, 3, 5...) reciben placas pares (0, 2, 4, 6, 8)
            // Lógica ajustada a la norma actual de días pares/impares
            bool esDiaPar = fecha.Day % 2 == 0;
            bool esPlacaPar = ultimoDigito % 2 == 0;

            return (esDiaPar == esPlacaPar);
        }
    }
}