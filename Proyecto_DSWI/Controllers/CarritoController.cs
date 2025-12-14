using Microsoft.AspNetCore.Mvc;
using Proyecto_DSWI.Models;
using Proyecto_DSWI.Extensions; // Importar la extensión de sesión
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims; // Para leer el ID del usuario

namespace Proyecto_DSWI.Controllers
{
    [Authorize] // Solo usuarios logueados pueden comprar
    public class CarritoController : Controller
    {
        private readonly AppDbContext _context;
        const string SessionKey = "CarritoCompras"; // Clave para la sesión

        public CarritoController(AppDbContext context)
        {
            _context = context;
        }

        // 1. VISTA: Ver qué hay en el carrito
        public IActionResult Index()
        {
            var carrito = HttpContext.Session.GetObjectFromJson<List<CarritoItemViewModel>>(SessionKey)
                          ?? new List<CarritoItemViewModel>();
            return View(carrito);
        }

        // 2. ACCIÓN: Agregar producto al carrito
        public async Task<IActionResult> Agregar(int id)
        {
            // Buscar producto en BD para obtener precio y nombre
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null) return NotFound();

            // Recuperar carrito actual
            var carrito = HttpContext.Session.GetObjectFromJson<List<CarritoItemViewModel>>(SessionKey)
                          ?? new List<CarritoItemViewModel>();

            // Verificar si ya existe en el carrito
            var itemExistente = carrito.FirstOrDefault(c => c.ProductoId == id);

            if (itemExistente != null)
            {
                itemExistente.Cantidad++; // Si existe, suma 1
            }
            else
            {
                carrito.Add(new CarritoItemViewModel
                {
                    ProductoId = producto.ProductoId,
                    NombreProducto = producto.Nombre,
                    Precio = producto.PrecioLista ?? 0,
                    Cantidad = 1
                });
            }

            // Guardar carrito actualizado en sesión
            HttpContext.Session.SetObjectAsJson(SessionKey, carrito);

            return RedirectToAction("Index"); // Ir a la vista del carrito
        }

        // 3. ACCIÓN: Finalizar Compra (TRANSACCIÓN)
        [HttpPost]
        public async Task<IActionResult> ProcesarCompra()
        {
            var carrito = HttpContext.Session.GetObjectFromJson<List<CarritoItemViewModel>>(SessionKey);
            if (carrito == null || !carrito.Any()) return RedirectToAction("Index");

            // --- INICIO TRANSACCIÓN (Requisito Rúbrica) ---
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // A. Guardar Cabecera (Pedido)
                    // Nota: Asegúrate de tener el User ID correcto. Aquí uso uno fijo (2) como ejemplo, 
                    // pero deberías sacarlo de User.Claims si configuraste bien el Login.
                    long usuarioId = 2;

                    var pedido = new Pedido
                    {
                        UsuarioId = usuarioId,
                        Fecha = DateTime.Now,
                        Total = carrito.Sum(x => x.Subtotal),
                        Estado = "PAGADO"
                    };

                    _context.Pedidos.Add(pedido);
                    await _context.SaveChangesAsync(); // Genera el ID

                    // B. Guardar Detalles
                    foreach (var item in carrito)
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
                    }
                    await _context.SaveChangesAsync();

                    // C. Confirmar
                    transaction.Commit();

                    // D. Limpiar Carrito
                    HttpContext.Session.Remove(SessionKey);

                    return RedirectToAction("CompraExitosa");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    ViewBag.Error = "Ocurrió un error: " + ex.Message;
                    return View("Index", carrito);
                }
            }
        }

        public IActionResult CompraExitosa()
        {
            return View();
        }
    }
}