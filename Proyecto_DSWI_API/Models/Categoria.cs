using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Proyecto_DSWI_API.Models
{
    [Table("categoria")]
    public class Categoria
    {
        [Key]
        [Column("categoria_id")]
        public short CategoriaId { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(80)]
        [Column("nombre")]
        public string? Nombre { get; set; }

        [StringLength(255)]
        [Column("descripcion")]
        public string? Descripcion { get; set; }

        [Column("activo")]
        public bool Activo { get; set; }

        [Column("ultima_actualizacion")]
        public DateTime UltimaActualizacion { get; set; }


        public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}
