using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControlTaxisApp.Models
{
    [Table("Vehiculos")]
    public class Vehiculo
    {
        [Key]
        public int Id { get; set; }
        public string Placa { get; set; } = null!;
        public string? Modelo { get; set; }
    }
}