using Microsoft.AspNetCore.Mvc;
using Proyecto_DSWI_API.Interfaces;
using Proyecto_DSWI_API.Models;

namespace Proyecto_DSWI_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosController : ControllerBase
    {
        private readonly IProducto _productoDao;

        public ProductosController(IProducto productoDao)
        {
            _productoDao = productoDao;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _productoDao.ListarProductos());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var prod = await _productoDao.ObtenerProducto(id);
            if (prod == null) return NotFound();
            return Ok(prod);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Producto producto)
        {
            var resultado = await _productoDao.InsertarProducto(producto);
            return Ok(new { mensaje = resultado });
        }
    }
}
