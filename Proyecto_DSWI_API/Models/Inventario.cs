using System.ComponentModel.DataAnnotations.Schema;

namespace Proyecto_DSWI_API.Models
{
    [Table("inventario")]
    public class Inventario
    {


        [Column("almacen_id")]
        public short AlmacenId { get; set; }

        [ForeignKey("AlmacenId")]
        public virtual Almacen? Almacen { get; set; }

        [Column("producto_id")]
        public int ProductoId { get; set; }

        [ForeignKey("ProductoId")]
        public virtual Producto? Producto { get; set; }

        [Column("cantidad")]
        public decimal Cantidad { get; set; }

        [Column("stock_min")]
        public decimal StockMin { get; set; }

        [Column("stock_max")]
        public decimal? StockMax { get; set; }

        [Column("ultima_actualizacion")]
        public DateTime UltimaActualizacion { get; set; }
    }
}
