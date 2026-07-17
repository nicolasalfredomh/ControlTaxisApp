namespace ControlTaxisApp.Models.ViewModels
{
    public class ReporteConsolidadoViewModel
    {
        public List<LiquidacionDiaria> Liquidaciones { get; set; } = new();
        public List<Mantenimiento> Mantenimientos { get; set; } = new(); // Asegúrate de tener el modelo Mantenimiento

        public List<GastoAdministrativo> GastosAdministrativos { get; set; } = new();

        public decimal TotalProducido => Liquidaciones.Sum(l => l.Producido);
        public decimal TotalGastosLiquidacion => Liquidaciones.Sum(l => l.Gastos) ?? 0;
        public decimal TotalGastosMantenimiento => Mantenimientos.Sum(m => m.Valor); // Ajusta según tu propiedad de costo
        public decimal UtilidadNeta => TotalProducido - TotalGastosLiquidacion ;
    }
}