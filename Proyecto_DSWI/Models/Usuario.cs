using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Proyecto_DSWI.Models
{
    // Mapeo a la tabla usuario
    [Table("usuario")]
    public class Usuario
    {
        // PK: usuario_id BIGINT
        [Key]
        [Column("usuario_id")]
        public long UsuarioId { get; set; }

        [Required]
        [StringLength(80)]
        [Column("nombre")]
        public string? Nombre { get; set; }

        [Required]
        [StringLength(100)]
        [Column("email")]
        public string? Email { get; set; } // Lo usaremos como nombre de usuario para el login

        [Required]
        [StringLength(255)]
        [Column("password_hash")]
        public string? PasswordHash { get; set; }

        [Column("enabled")]
        public bool Enabled { get; set; } = true;

        // Propiedad de navegación Many-to-Many (hacia Rol)
        public virtual ICollection<Rol> Roles { get; set; } = new List<Rol>();
    }
}