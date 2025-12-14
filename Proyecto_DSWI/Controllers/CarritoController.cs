using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proyecto_DSWI.Extensions; // Tu clase para Session
using Proyecto_DSWI.Models;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Proyecto_DSWI.Controllers
{
    [Authorize]
    public class CarritoController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        const string SessionKey = "CarritoCompras";

        public CarritoController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult Index()
        {
            var carrito = HttpContext.Session.GetObjectFromJson<List<CarritoItemViewModel>>(SessionKey)
                          ?? new List<CarritoItemViewModel>();
            return View(carrito);
        }

        public async Task<IActionResult> Agregar(int id)
        {
            // OJO: Aquí deberíamos pedir el producto a la API para ver el precio real,
            // pero por simplicidad puedes dejarlo o usar _httpClientFactory.
            // Si quieres hacerlo rápido, asume que recibes los datos necesarios o consúmelo:

            var client = _httpClientFactory.CreateClient("TiendaAPI");
            var response = await client.GetAsync($"api/productos/{id}");

            if (!response.IsSuccessStatusCode) return NotFound();

            var jsonString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var producto = JsonSerializer.Deserialize<Producto>(jsonString, options);

            var carrito = HttpContext.Session.GetObjectFromJson<List<CarritoItemViewModel>>(SessionKey)
                          ?? new List<CarritoItemViewModel>();

            var itemExistente = carrito.FirstOrDefault(c => c.ProductoId == id);
            if (itemExistente != null)
            {
                itemExistente.Cantidad++;
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
            HttpContext.Session.SetObjectAsJson(SessionKey, carrito);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ProcesarCompra()
        {
            var carrito = HttpContext.Session.GetObjectFromJson<List<CarritoItemViewModel>>(SessionKey);
            if (carrito == null || !carrito.Any()) return RedirectToAction("Index");

            // Preparar el DTO para la API
            // Nota: Asegúrate de obtener el ID real del usuario logueado
            long userId = 0;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userIdClaim, out userId)) userId = 1; // Fallback por si falla el login

            var ventaDto = new
            {
                UsuarioId = userId,
                Total = carrito.Sum(x => x.Subtotal),
                Detalles = carrito.Select(x => new {
                    ProductoId = x.ProductoId,
                    Cantidad = x.Cantidad,
                    Precio = x.Precio,
                    Subtotal = x.Subtotal
                }).ToList()
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(ventaDto), Encoding.UTF8, "application/json");

            var client = _httpClientFactory.CreateClient("TiendaAPI");
            var response = await client.PostAsync("api/ventas/procesar", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                HttpContext.Session.Remove(SessionKey);
                return RedirectToAction("CompraExitosa");
            }
            else
            {
                ViewBag.Error = "Error en API: " + await response.Content.ReadAsStringAsync();
                return View("Index", carrito);
            }
        }

        public IActionResult CompraExitosa() => View();
    }
}