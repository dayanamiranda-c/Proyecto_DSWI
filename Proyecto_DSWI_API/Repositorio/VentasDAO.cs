namespace Proyecto_DSWI_API.Repositorio
{
    public class VentasDAO : IVentas
    {
        private readonly AppDbContext _context;

        public VentasDAO(AppDbContext context)
        {
            _context = context;
        }

        public async Task<string> RegistrarVenta(VentaDTO venta)
        {
            // REQUISITO IV: Uso de Transacciones
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // 1. Cabecera (Pedido)
                    var pedido = new Pedido
                    {
                        UsuarioId = venta.UsuarioId,
                        Fecha = DateTime.Now,
                        Total = venta.Total,
                        Estado = "PAGADO"
                    };
                    _context.Pedidos.Add(pedido);
                    await _context.SaveChangesAsync(); // Genera el ID

                    // 2. Detalles
                    foreach (var item in venta.Detalles)
                    {
                        var detalle = new DetallePedido
                        {
                            PedidoId = pedido.PedidoId,
                            ProductoId = item.ProductoId,
                            Cantidad = item.Cantidad,
                            PrecioUnitario = item.Precio,
                            Subtotal = item.Subtotal
                        };
                        _context.DetallesPedido.Add(detalle);

                        // Opcional: Aquí podrías restar stock
                    }
                    await _context.SaveChangesAsync();

                    // 3. Confirmar Transacción
                    transaction.Commit();
                    return "OK";
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return "Error en la transacción: " + ex.Message;
                }
            }
        }
    }
}
