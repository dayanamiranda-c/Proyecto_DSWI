using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Proyecto_DSWI.Models;
using System.Security.Claims; // Para obtener el ID del usuario logueado

namespace Proyecto_DSWI.Controllers
{
    [Authorize]
    public class VentasController : Controller
    {
        private readonly AppDbContext _context;

        public VentasController(AppDbContext context)
        {
            _context = context;
        }

        // Acción para finalizar la compra (El punto clave del proyecto)
        [HttpPost]
        public async Task<IActionResult> FinalizarCompra(List<CarritoItemViewModel> itemsCarrito)
        {
            if (itemsCarrito == null || itemsCarrito.Count == 0) return RedirectToAction("Index", "Catalogo");

            // 1. INICIAR TRANSACCIÓN (Requisito Obligatorio)
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // 2. Crear la Cabecera (Pedido)
                    var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)); // Asumiendo que guardaste el ID en los Claims al loguear

                    var pedido = new Pedido
                    {
                        UsuarioId = userId,
                        Fecha = DateTime.Now,
                        Estado = "PAGADO",
                        Total = itemsCarrito.Sum(x => x.Subtotal)
                    };

                    _context.Pedidos.Add(pedido);
                    await _context.SaveChangesAsync(); // Aquí se genera el PedidoId

                    // 3. Crear los Detalles
                    foreach (var item in itemsCarrito)
                    {
                        var detalle = new DetallePedido
                        {
                            PedidoId = pedido.PedidoId, // Usamos el ID generado arriba
                            ProductoId = item.ProductoId,
                            Cantidad = item.Cantidad,
                            PrecioUnitario = item.Precio,
                            Subtotal = item.Subtotal
                        };
                        _context.DetallesPedido.Add(detalle);

                        // Opcional: Aquí deberías restar el stock del inventario también
                    }

                    await _context.SaveChangesAsync();

                    // 4. CONFIRMAR TRANSACCIÓN
                    transaction.Commit();

                    // Limpiar carrito de sesión y redirigir
                    return RedirectToAction("Confirmacion");
                }
                catch (Exception ex)
                {
                    // Si algo falla, deshace todo (Rollback automático al no hacer Commit)
                    transaction.Rollback();
                    ModelState.AddModelError("", "Error al procesar la venta: " + ex.Message);
                    return View("Carrito", itemsCarrito);
                }
            }
        }
    }
}