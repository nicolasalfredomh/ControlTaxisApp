using System.ComponentModel.DataAnnotations;

namespace ControlTaxisApp.Models
{
    public class TipoMantenimiento
    {
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; } // Ej: "Motor", "Frenos", etc.
    }
}