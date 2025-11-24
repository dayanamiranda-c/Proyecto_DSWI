using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proyecto_DSWI.Models
{
    [Table("producto")]
    public class Producto
    {
        [Key]
        [Column("producto_id")]
        public int ProductoId { get; set; }

        [Required]
        [StringLength(30)]
        [Column("sku")]
        public string? Sku { get; set; }

        [Required(ErrorMessage = "El nombre del producto es obligatorio")]
        [StringLength(120)]
        [Column("nombre")]
        public string? Nombre { get; set; }

        [StringLength(500)]
        [Column("descripcion")]
        public string? Descripcion { get; set; } // Puede ser nulo

   
        [Column("categoria_id")]
        public short CategoriaId { get; set; }

        [StringLength(50)]
        [Column("codigo_barras")]
        public string? CodigoBarras { get; set; }

        [Column("precio_lista", TypeName = "decimal(18, 2)")]
        public decimal? PrecioLista { get; set; }

        [Column("activo")]
        public bool Activo { get; set; }

        [Column("creado_en")]
        public DateTime CreadoEn { get; set; }

        [Column("ultima_actualizacion")]
        public DateTime UltimaActualizacion { get; set; }

      
        [ForeignKey("CategoriaId")]
        public virtual Categoria? Categoria { get; set; }
    }
}