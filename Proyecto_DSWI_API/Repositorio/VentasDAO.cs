using Microsoft.EntityFrameworkCore;          // NECESARIO para transacciones y EF
using Proyecto_DSWI_API.Data;                 // NECESARIO para AppDbContext
using Proyecto_DSWI_API.DTOs;                 // NECESARIO para VentaDTO
using Proyecto_DSWI_API.Interfaces;           // NECESARIO para IVentas
using Proyecto_DSWI_API.Models;               // NECESARIO para Pedido y DetallePedido
using System;
using System.Threading.Tasks;

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
            // BeginTransactionAsync es mejor para alta concurrencia en Azure
            using (var transaction = await _context.Database.BeginTransactionAsync())
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
                    await _context.SaveChangesAsync(); // Genera el ID del pedido automáticamente

                    // 2. Detalles
                    foreach (var item in venta.Detalles)
                    {
                        var detalle = new DetallePedido
                        {
                            PedidoId = pedido.PedidoId, // Usamos el ID recién creado
                            ProductoId = item.ProductoId,
                            Cantidad = item.Cantidad,
                            PrecioUnitario = item.Precio,
                            Subtotal = item.Subtotal
                        };

                        // SI AQUÍ SALE ERROR: Revisa AppDbContext.cs y asegúrate 
                        // que DetallesPedido sea 'DbSet<DetallePedido>', NO 'object'
                        _context.DetallesPedido.Add(detalle);
                    }

                    await _context.SaveChangesAsync();

                    // 3. Confirmar Transacción
                    await transaction.CommitAsync();
                    return "OK";
                }
                catch (Exception ex)
                {
                    // Si algo falla, deshacemos todo
                    await transaction.RollbackAsync();
                    return "Error en la transacción: " + ex.Message;
                }
            }
        }
    }
}