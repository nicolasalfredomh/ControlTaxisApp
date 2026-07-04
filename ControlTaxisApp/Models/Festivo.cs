using System.ComponentModel.DataAnnotations;

namespace ControlTaxisApp.Models
{
    public class Festivo
    {
        public int Id { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; }

        [StringLength(100)]
        public string? Nombre { get; set; }
    }
}