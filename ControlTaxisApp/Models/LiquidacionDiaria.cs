using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControlTaxisApp.Models
{
    [Table("LiquidacionesDiarias")]
    public class LiquidacionDiaria
    {
        [Key]
        public int Id { get; set; }

        public int VehiculoId { get; set; }
        public int? ConductorId { get; set; }

        public DateTime Fecha { get; set; }
        public string? Descripcion { get; set; }

        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C0}", ApplyFormatInEditMode = false)]
        public decimal Producido { get; set; }

        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C0}", ApplyFormatInEditMode = false)]
        public decimal? Gastos { get; set; }

        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C0}", ApplyFormatInEditMode = false)]
        public decimal? Ahorro { get; set; }



        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C0}", ApplyFormatInEditMode = false)]
        public decimal Saldo { get; set; }
        public bool PicoYPlaca { get; set; }
    

        // Propiedades de navegación para hacer cruces de datos
        [ForeignKey("VehiculoId")]
        public virtual Vehiculo? Vehiculo { get; set; }

        [ForeignKey("ConductorId")]
        public virtual Conductor? Conductor { get; set; }



    }
}