using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Proyecto_DSWI.Models;
using System.Text.Json;
using System.Text;

namespace Proyecto_DSWI.Controllers
{
    [Authorize]
    public class CatalogoController : Controller
    {
        // Solo inyectamos HttpClient, nada de AppDbContext
        private readonly IHttpClientFactory _httpClientFactory;

        public CatalogoController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Inicio()
        {
            return View();
        }

        // -------------------------------------------------------------------
        // 1. INDEX: LISTADO DESDE API
        // -------------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("TiendaAPI");
            var response = await client.GetAsync("api/productos");

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var productos = JsonSerializer.Deserialize<List<Producto>>(jsonString, options);
                return View(productos);
            }

            ViewBag.Error = "No se pudo conectar con la API de productos.";
            return View(new List<Producto>());
        }

        // -------------------------------------------------------------------
        // 2. NUEVO (GET): CARGA CATEGORIAS DESDE API
        // -------------------------------------------------------------------
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Nuevo()
        {
            await CargarCategoriasViewBag(); // Refactorizado en un método privado abajo
            return View();
        }

        // -------------------------------------------------------------------
        // 3. NUEVO (POST): GUARDA VÍA API
        // -------------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> Nuevo(Producto producto)
        {
            // Validaciones básicas
            if (ModelState.IsValid)
            {
                // TRUCO: Creamos un objeto "limpio" para enviar a la API.
                // Esto evita errores de fechas, nulos o nombres incorrectos.
                var productoParaEnviar = new
                {
                    Sku = producto.Sku,
                    Nombre = producto.Nombre,
                    Descripcion = producto.Descripcion ?? "Sin descripción",
                    CategoriaId = producto.CategoriaId,

                    // OJO: La API suele esperar "Precio" pero el modelo MVC tiene "PrecioLista"
                    Precio = producto.PrecioLista,

                    // Valor por defecto para el stock inicial (ya que no lo pides en el formulario)
                    Stock = 50,
                    StockInicial = 50 // Mandamos ambos nombres por si acaso usaste DTOs
                };

                // Serializamos
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(productoParaEnviar),
                    Encoding.UTF8,
                    "application/json");

                // Enviamos a la API
                var client = _httpClientFactory.CreateClient("TiendaAPI");
                var response = await client.PostAsync("api/productos", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // Leemos el error real de la API para mostrarlo en pantalla
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", "Error API: " + errorMsg);
                }
            }

            // Si falla, recargamos el combo (usando la API como hicimos antes)
            await CargarCategoriasViewBag();
            return View(producto);
        }

        // Método auxiliar para no repetir código de carga de categorías
        private async Task CargarCategoriasViewBag()
        {
            var client = _httpClientFactory.CreateClient("TiendaAPI");
            var response = await client.GetAsync("api/categorias");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var categorias = JsonSerializer.Deserialize<List<Categoria>>(json, options);
                ViewBag.Categorias = new SelectList(categorias, "CategoriaId", "Nombre");
            }
            else
            {
                ViewBag.Categorias = new SelectList(new List<Categoria>(), "CategoriaId", "Nombre");
            }
        }

        // -------------------------------------------------------------------
        // 4. REPORTES (COMENTADOS PORQUE REQUIEREN API NUEVA O DB)
        // -------------------------------------------------------------------
        // Nota: Para que estos funcionen sin AppDbContext, debes crear 
        // endpoints en tu API como "api/movimientos" y "api/inventario"
        /*
        [HttpGet]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> Movimientos()
        {
             // TODO: Consumir API cuando exista el endpoint
             return View(new List<InventarioMovimiento>());
        }

        [HttpGet]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> Inventario(short? id)
        {
             // TODO: Consumir API cuando exista el endpoint
             return View(new List<Inventario>());
        }
        */

        // ... (Tu código anterior: Index, Nuevo, etc.) ...

        // -------------------------------------------------------------------
        // 5. INVENTARIO (GET): Consumo de API
        // -------------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Inventario(int? id)
        {
            var client = _httpClientFactory.CreateClient("TiendaAPI");

            // Llamamos a la API enviando el ID del almacén si existe
            string url = id.HasValue ? $"api/inventario?almacenId={id}" : "api/inventario";
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                // Usamos el DTO que creamos (debes crear una clase igual en Models del MVC o usar dynamic)
                // Para rápido, usaremos una clase anónima o View específica.
                var data = JsonSerializer.Deserialize<List<InventarioViewModel>>(json, options);

                return View(data);
            }

            return View(new List<InventarioViewModel>());
        }

        // -------------------------------------------------------------------
        // 6. EDITAR (POST): Envía actualización a la API (PUT)
        // -------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(Producto producto)
        {
            // Nota: Validamos lo básico. En producción validarias ModelState.IsValid completo

            // Creamos el DTO que espera la API (ProductoUpdateDTO)
            var productoUpdate = new
            {
                ProductoId = producto.ProductoId,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion ?? "",
                CategoriaId = producto.CategoriaId,
                Precio = producto.PrecioLista // Mapeamos PrecioLista a Precio
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(productoUpdate),
                Encoding.UTF8,
                "application/json");

            var client = _httpClientFactory.CreateClient("TiendaAPI");

            // PUT: api/productos
            var response = await client.PutAsync("api/productos", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index)); // Volver a la tabla
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", "Error API: " + error);
            }

            await CargarCategoriasViewBag();
            return View(producto);
        }

        // -------------------------------------------------------------------
        // 7. ELIMINAR (GET): Muestra confirmación
        // -------------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Eliminar(int id)
        {
            var client = _httpClientFactory.CreateClient("TiendaAPI");
            var response = await client.GetAsync($"api/productos/{id}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var producto = JsonSerializer.Deserialize<Producto>(json, options);
                return View(producto);
            }
            return NotFound();
        }

        // -------------------------------------------------------------------
        // 8. ELIMINAR (POST): Ejecuta borrado en API (DELETE)
        // -------------------------------------------------------------------
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var client = _httpClientFactory.CreateClient("TiendaAPI");

            // DELETE: api/productos/{id}
            var response = await client.DeleteAsync($"api/productos/{id}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Error = "No se pudo eliminar el producto.";
            return RedirectToAction(nameof(Index));
        }
        // -------------------------------------------------------------------
        // 6. MOVIMIENTOS (GET): Consumo de API
        // -------------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Movimientos()
        {
            var client = _httpClientFactory.CreateClient("TiendaAPI");
            var response = await client.GetAsync("api/movimientos");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var historial = JsonSerializer.Deserialize<List<MovimientoViewModel>>(json, options);
                return View(historial);
            }

            return View(new List<MovimientoViewModel>());
        }
    }
}