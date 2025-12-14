using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Proyecto_DSWI_API.Models
{
    [Table("detalle_pedido")]
    public class DetallePedido
    {
        [Key]
        [Column("detalle_id")]
        public long DetalleId { get; set; }

        [Column("pedido_id")]
        public long PedidoId { get; set; }

        [ForeignKey("PedidoId")]
        public virtual Pedido? Pedido { get; set; }

        [Column("producto_id")]
        public int ProductoId { get; set; }

        [ForeignKey("ProductoId")]
        public virtual Producto? Producto { get; set; }

        [Column("cantidad")]
        public int Cantidad { get; set; }

        [Column("precio_unitario", TypeName = "decimal(18, 2)")]
        public decimal PrecioUnitario { get; set; }

        [Column("subtotal", TypeName = "decimal(18, 2)")]
        public decimal Subtotal { get; set; }
    }
}
