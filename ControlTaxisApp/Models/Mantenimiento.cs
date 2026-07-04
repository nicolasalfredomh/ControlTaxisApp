using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControlTaxisApp.Models
{
    public class Mantenimiento
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Vehículo")]
        public int VehiculoId { get; set; }

      
        [StringLength(250)]
        public string? Descripcion { get; set; } // Ej: "Cambio de aceite y filtro", "Pastillas de frenos"

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Valor { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        // Propiedad de navegación para relacionarlo con la tabla Vehiculo
        [ForeignKey("VehiculoId")]
        public virtual Vehiculo? IdVehiculoNavigation { get; set; }


        // Campos opcionales (sin [Required] y con ?)
        public int? Kilometraje { get; set; }
        public int? ProximoCambio { get; set; }
        public string? Taller { get; set; }
        public string? Garantia { get; set; }
        public decimal? Iva { get; set; }

        public int TipoMantenimientoId { get; set; }
        public TipoMantenimiento? TipoMantenimiento { get; set; }
    }
}