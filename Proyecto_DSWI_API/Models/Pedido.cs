using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Proyecto_DSWI_API.Models
{
    [Table("pedido")]
    public class Pedido
    {
        [Key]
        [Column("pedido_id")]
        public long PedidoId { get; set; }

        [Column("usuario_id")]
        public long UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public virtual Usuario? Usuario { get; set; }

        [Column("fecha")]
        public DateTime Fecha { get; set; }

        [Column("total", TypeName = "decimal(18, 2)")]
        public decimal Total { get; set; }

        [Column("estado")]
        public string? Estado { get; set; }

        // Relación con detalles
        public virtual ICollection<DetallePedido> Detalles { get; set; } = new List<DetallePedido>();
    }
}
