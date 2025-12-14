using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto_DSWI_API.Data;

namespace Proyecto_DSWI_API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthAPIController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthAPIController(AppDbContext context)
        {
            _context = context;
        }

        // ESTE METODO ES EL IMPORTANTE
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // 1. Validar que vengan datos
            if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                return BadRequest("Datos incompletos");

            // 2. Buscar usuario en BD (Incluyendo Roles)
            var usuario = await _context.Usuarios
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            // 3. Validar Contraseña (Comparación directa según tu script)
            if (usuario == null || usuario.PasswordHash != request.Password)
            {
                return Unauthorized(new { mensaje = "Credenciales incorrectas" });
            }

            // 4. Retornar éxito
            return Ok(new
            {
                usuario.UsuarioId,
                usuario.Nombre,
                usuario.Email,
                Roles = usuario.Roles.Select(r => r.Nombre).ToList()
            });
        }
    }

    // Clase para recibir los datos del JSON
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}