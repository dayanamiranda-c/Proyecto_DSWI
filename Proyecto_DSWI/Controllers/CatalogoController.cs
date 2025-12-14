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
            if (ModelState.IsValid)
            {
                // Datos automáticos
                producto.CreadoEn = DateTime.Now;
                producto.UltimaActualizacion = DateTime.Now;
                producto.Activo = true;
                if (string.IsNullOrEmpty(producto.CodigoBarras))
                    producto.CodigoBarras = "GEN-" + DateTime.Now.Ticks;

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(producto),
                    Encoding.UTF8,
                    "application/json");

                var client = _httpClientFactory.CreateClient("TiendaAPI");
                var response = await client.PostAsync("api/productos", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Error en la API al guardar el producto.");
                }
            }

            // CORRECCIÓN IMPORTANTE:
            // Si falla, recargamos las categorías usando la API, NO usando _context
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
    }
}