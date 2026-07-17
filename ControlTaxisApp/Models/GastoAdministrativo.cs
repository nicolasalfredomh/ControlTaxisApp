namespace ControlTaxisApp.Models
{
    public class GastoAdministrativo
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Concepto { get; set; } // Ejemplo: Pago internet, Papelería
        public decimal Valor { get; set; }
        public string Observaciones { get; set; }


        public string Placa { get; set; }

    }
}
