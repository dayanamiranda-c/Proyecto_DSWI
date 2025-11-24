using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proyecto_DSWI.Models
{
    [Table("inventario_movimiento")]
    public class InventarioMovimiento
    {
        [Key]
        [Column("inventario_movimiento_id")]
        public long InventarioMovimientoId { get; set; }

        [Column("fecha_movimiento")]
        public DateTime FechaMovimiento { get; set; }

        [Column("almacen_id")]
        public short AlmacenId { get; set; }

        [ForeignKey("AlmacenId")]
        public virtual Almacen? Almacen { get; set; }

        [Column("producto_id")]
        public int ProductoId { get; set; }

        [ForeignKey("ProductoId")]
        public virtual Producto? Producto { get; set; }

        [Required]
        [StringLength(20)]
        [Column("tipo_movimiento")]
        public string? TipoMovimiento { get; set; } 

        [Column("cantidad")]
        public decimal Cantidad { get; set; }

        [Column("costo_unitario")]
        public decimal? CostoUnitario { get; set; }

        [StringLength(100)]
        [Column("referencia")]
        public string? Referencia { get; set; }

        [StringLength(60)]
        [Column("usuario")]
        public string? Usuario { get; set; }

        [Column("ultima_actualizacion")]
        public DateTime UltimaActualizacion { get; set; }
    }
}