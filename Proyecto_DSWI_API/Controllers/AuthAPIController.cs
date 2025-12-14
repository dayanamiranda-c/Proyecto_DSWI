using Microsoft.AspNetCore.Mvc;

namespace Proyecto_DSWI_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // CAMBIO: Heredar de ControllerBase (más ligero para APIs)
    public class AuthAPIController : ControllerBase
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Ok(new { mensaje = "Auth API funcionando" });
        }

        // Aquí deberías poner tu lógica de Login (POST) que devuelve un Token o el Usuario
    }
}
