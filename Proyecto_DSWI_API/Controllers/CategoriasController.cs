using Microsoft.AspNetCore.Mvc;
using Proyecto_DSWI_API.DTOs;
using Proyecto_DSWI_API.Interfaces;

namespace Proyecto_DSWI_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriasController : ControllerBase
    {
        private readonly ICategoria _dao;
        public CategoriasController(ICategoria dao) { _dao = dao; }

        [HttpGet]
        public async Task<IActionResult> Get() => Ok(await _dao.ListarCategorias());

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var obj = await _dao.ObtenerCategoria(id);
            return obj == null ? NotFound() : Ok(obj);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CategoriaCreateDTO dto)
        {
            var res = await _dao.InsertarCategoria(dto);
            return res == "OK" ? Ok(new { mensaje = "Categoría creada" }) : BadRequest(new { mensaje = res });
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] CategoriaUpdateDTO dto)
        {
            var res = await _dao.ActualizarCategoria(dto);
            return res == "OK" ? Ok(new { mensaje = "Categoría actualizada" }) : BadRequest(new { mensaje = res });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var res = await _dao.EliminarCategoria(id);
            return res == "OK" ? Ok(new { mensaje = "Categoría eliminada" }) : BadRequest(new { mensaje = res });
        }
    }
}