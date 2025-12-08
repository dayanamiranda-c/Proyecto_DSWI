using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Proyecto_DSWI.Models
{
    // Mapeo a la tabla rol
    [Table("rol")]
    public class Rol
    {
        // PK: rol_id BIGINT
        [Key]
        [Column("rol_id")]
        public long RolId { get; set; }

        [Required]
        [StringLength(50)]
        [Column("nombre")]
        public string? Nombre { get; set; }

        // Propiedad de navegación Many-to-Many
        public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
}