using Microsoft.AspNetCore.Mvc;
using Proyecto_DSWI_API.DTOs;
using Proyecto_DSWI_API.Interfaces;

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

        // POST: api/productos
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ProductoCreateDTO productoDto)
        {
            var resultado = await _productoDao.InsertarProducto(productoDto);
            if (resultado == "OK") return Ok(new { mensaje = "Producto registrado" });
            return BadRequest(new { mensaje = resultado });
        }

        // PUT: api/productos
        [HttpPut]
        public async Task<IActionResult> Put([FromBody] ProductoUpdateDTO productoDto)
        {
            var resultado = await _productoDao.ActualizarProducto(productoDto);
            if (resultado == "OK") return Ok(new { mensaje = "Producto actualizado" });
            return BadRequest(new { mensaje = resultado });
        }

        // DELETE: api/productos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var resultado = await _productoDao.EliminarProducto(id);
            if (resultado == "OK") return Ok(new { mensaje = "Producto eliminado" });
            return BadRequest(new { mensaje = resultado });
        }
    }
}