using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControlTaxisApp.Models
{
    [Table("Usuarios")] // Forzamos a que busque la tabla "Usuarios" a secas
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        public string NombreUsuario { get; set; } = null!;
        public string Clave { get; set; } = null!;
        public string NombreCompleto { get; set; } = null!;
    }
}