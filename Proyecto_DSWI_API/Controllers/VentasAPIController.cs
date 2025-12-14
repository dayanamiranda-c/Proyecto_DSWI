using Microsoft.AspNetCore.Mvc;
using Proyecto_DSWI_API.DTOs;
using Proyecto_DSWI_API.Interfaces;

namespace Proyecto_DSWI_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VentasController : ControllerBase
    {
        private readonly IVentas _ventasDao;

        public VentasController(IVentas ventasDao)
        {
            _ventasDao = ventasDao;
        }

        [HttpPost("procesar")]
        public async Task<IActionResult> Procesar([FromBody] VentaDTO venta)
        {
            string resultado = await _ventasDao.RegistrarVenta(venta);

            if (resultado == "OK")
                return Ok(new { mensaje = "Venta registrada con éxito" });
            else
                return BadRequest(new { mensaje = resultado });
        }
    }
}
