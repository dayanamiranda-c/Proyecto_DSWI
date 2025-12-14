using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proyecto_DSWI.Extensions; // Asegúrate de tener tu clase de extensiones de Sesión
using Proyecto_DSWI.Models;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Proyecto_DSWI.Controllers
{
    [Authorize] // Solo usuarios logueados pueden ver el carrito
    public class CarritoController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        const string SessionKey = "CarritoCompras";

        public CarritoController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // ---------------------------------------------------
        // 1. VER EL CARRITO (Lee de la memoria/sesión)
        // ---------------------------------------------------
        [HttpGet]
        public IActionResult Index()
        {
            var carrito = HttpContext.Session.GetObjectFromJson<List<CarritoItemViewModel>>(SessionKey)
                          ?? new List<CarritoItemViewModel>();
            return View(carrito);
        }

        // ---------------------------------------------------
        // 2. AGREGAR ITEM (Consulta API para datos frescos)
        // ---------------------------------------------------
        public async Task<IActionResult> Agregar(int id)
        {
            // 1. Pedir info del producto a la API (Precio y Nombre)
            var client = _httpClientFactory.CreateClient("TiendaAPI");
            var response = await client.GetAsync($"api/productos/{id}");

            if (!response.IsSuccessStatusCode) return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var producto = JsonSerializer.Deserialize<Producto>(json, options);

            // 2. Traer carrito actual de la sesión
            var carrito = HttpContext.Session.GetObjectFromJson<List<CarritoItemViewModel>>(SessionKey)
                          ?? new List<CarritoItemViewModel>();

            // 3. Buscar si ya existe el item
            var item = carrito.FirstOrDefault(c => c.ProductoId == id);
            if (item != null)
            {
                item.Cantidad++;
            }
            else
            {
                carrito.Add(new CarritoItemViewModel
                {
                    ProductoId = producto.ProductoId,
                    NombreProducto = producto.Nombre,
                    Precio = producto.PrecioLista ?? 0, // Usamos PrecioLista como Precio
                    Cantidad = 1
                });
            }

            // 4. Guardar carrito actualizado
            HttpContext.Session.SetObjectAsJson(SessionKey, carrito);

            return RedirectToAction("Index");
        }

        // ---------------------------------------------------
        // 3. ELIMINAR ITEM (Solo de memoria)
        // ---------------------------------------------------
        public IActionResult Eliminar(int id)
        {
            var carrito = HttpContext.Session.GetObjectFromJson<List<CarritoItemViewModel>>(SessionKey);
            if (carrito != null)
            {
                var item = carrito.FirstOrDefault(c => c.ProductoId == id);
                if (item != null)
                {
                    carrito.Remove(item);
                    HttpContext.Session.SetObjectAsJson(SessionKey, carrito);
                }
            }
            return RedirectToAction("Index");
        }

        // ---------------------------------------------------
        // 4. PROCESAR COMPRA (Envía todo a la API)
        // ---------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> ProcesarCompra()
        {
            // A. Recuperar carrito
            var carrito = HttpContext.Session.GetObjectFromJson<List<CarritoItemViewModel>>(SessionKey);
            if (carrito == null || !carrito.Any()) return RedirectToAction("Index");

            // B. Obtener ID del Usuario Logueado (desde la Cookie)
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            long usuarioId = long.Parse(userIdClaim ?? "0"); // Si es 0, fallará la FK en BD

            // C. Armar el objeto VentaDTO (Estructura que espera la API)
            var ventaDto = new
            {
                UsuarioId = usuarioId,
                Total = carrito.Sum(x => x.Subtotal),
                Detalles = carrito.Select(item => new
                {
                    ProductoId = item.ProductoId,
                    Cantidad = item.Cantidad,
                    Precio = item.Precio,
                    Subtotal = item.Subtotal
                }).ToList()
            };

            // D. Enviar a la API
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(ventaDto),
                Encoding.UTF8,
                "application/json");

            var client = _httpClientFactory.CreateClient("TiendaAPI");
            var response = await client.PostAsync("api/ventas/procesar", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                // ¡Éxito! Limpiamos el carrito y mostramos confirmación
                HttpContext.Session.Remove(SessionKey);
                return RedirectToAction("CompraExitosa");
            }
            else
            {
                // Error: Mostramos mensaje de la API
                var errorMsg = await response.Content.ReadAsStringAsync();
                TempData["Error"] = "No se pudo procesar la compra: " + errorMsg;
                return RedirectToAction("Index");
            }
        }
        // ---------------------------------------------------
        // 5. DISMINUIR CANTIDAD (Resta 1 o elimina si es 0)
        // ---------------------------------------------------
        public IActionResult Disminuir(int id)
        {
            var carrito = HttpContext.Session.GetObjectFromJson<List<CarritoItemViewModel>>(SessionKey);

            if (carrito != null)
            {
                var item = carrito.FirstOrDefault(c => c.ProductoId == id);
                if (item != null)
                {
                    if (item.Cantidad > 1)
                    {
                        item.Cantidad--; // Restamos 1
                    }
                    else
                    {
                        carrito.Remove(item); // Si llega a 0, lo quitamos del carrito
                    }
                    HttpContext.Session.SetObjectAsJson(SessionKey, carrito);
                }
            }
            return RedirectToAction("Index");
        }
        public IActionResult CompraExitosa() => View();
    }
}