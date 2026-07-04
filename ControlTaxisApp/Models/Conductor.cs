using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControlTaxisApp.Models
{
    [Table("Conductores")]
    public class Conductor
    {
        [Key]
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public bool Activo { get; set; }
    }
}