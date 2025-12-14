using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Proyecto_DSWI_API.Models
{
    [Table("almacen")]
    public class Almacen
    {
        [Key]
        [Column("almacen_id")]
        public short AlmacenId { get; set; }

        [Required]
        [StringLength(80)]
        [Column("nombre")]
        public string? Nombre { get; set; }

        [Required]
        [StringLength(20)]
        [Column("tipo")]
        public string? Tipo { get; set; }

        [Column("direccion")]
        public string? Direccion { get; set; }

        [Column("activo")]
        public bool Activo { get; set; }

        [Column("ultima_actualizacion")]
        public DateTime UltimaActualizacion { get; set; }
    }
}
