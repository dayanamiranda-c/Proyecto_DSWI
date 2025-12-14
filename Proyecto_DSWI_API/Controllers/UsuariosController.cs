using Microsoft.AspNetCore.Mvc;
using Proyecto_DSWI_API.DTOs;
using Proyecto_DSWI_API.Interfaces;

namespace Proyecto_DSWI_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuario _dao;
        public UsuariosController(IUsuario dao) { _dao = dao; }

        [HttpGet]
        public async Task<IActionResult> Get() => Ok(await _dao.ListarUsuarios());

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(long id)
        {
            var obj = await _dao.ObtenerUsuario(id);
            return obj == null ? NotFound() : Ok(obj);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UsuarioCreateDTO dto)
        {
            var res = await _dao.RegistrarUsuario(dto);
            return res == "OK" ? Ok(new { mensaje = "Usuario registrado" }) : BadRequest(new { mensaje = res });
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] UsuarioUpdateDTO dto)
        {
            var res = await _dao.ActualizarUsuario(dto);
            return res == "OK" ? Ok(new { mensaje = "Usuario actualizado" }) : BadRequest(new { mensaje = res });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var res = await _dao.DesactivarUsuario(id);
            return res == "OK" ? Ok(new { mensaje = "Usuario desactivado" }) : BadRequest(new { mensaje = res });
        }
    }
}