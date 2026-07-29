using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControlTaxisApp.Models
{
    public class Conductor
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; }

        public string Apellidos { get; set; }
        public string Telefono { get; set; }
        public string Direccion { get; set; }


        // Guardará la ruta o nombre del archivo de la foto
        public string FotoUrl { get; set; }

        public bool Activo { get; set; } = true;

        // Llave foránea para vincular el conductor al taxi/vehículo
        [Display(Name = "Vehículo Asignado")]
        public int? VehiculoId { get; set; }

        [ForeignKey("VehiculoId")]
        public virtual Vehiculo Vehiculo { get; set; }
    }
}