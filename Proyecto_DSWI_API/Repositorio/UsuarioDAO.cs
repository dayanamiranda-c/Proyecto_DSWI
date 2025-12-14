using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Proyecto_DSWI_API.Data;
using Proyecto_DSWI_API.DTOs;
using Proyecto_DSWI_API.Interfaces;
using Proyecto_DSWI_API.Models;

namespace Proyecto_DSWI_API.Repositorio
{
    public class UsuarioDAO : IUsuario
    {
        private readonly AppDbContext _context;
        public UsuarioDAO(AppDbContext context) { _context = context; }

        public async Task<IEnumerable<UsuarioResponseDTO>> ListarUsuarios()
        {
            // Proyección manual para no devolver password
            return await _context.Usuarios
                .Include(u => u.Roles)
                .Select(u => new UsuarioResponseDTO
                {
                    UsuarioId = u.UsuarioId,
                    Nombre = u.Nombre,
                    Email = u.Email,
                    Enabled = u.Enabled,
                    Roles = u.Roles.Select(r => r.Nombre).ToList()
                }).ToListAsync();
        }

        public async Task<UsuarioResponseDTO> ObtenerUsuario(long id)
        {
            var u = await _context.Usuarios.Include(u => u.Roles).FirstOrDefaultAsync(x => x.UsuarioId == id);
            if (u == null) return null;
            return new UsuarioResponseDTO
            {
                UsuarioId = u.UsuarioId,
                Nombre = u.Nombre,
                Email = u.Email,
                Enabled = u.Enabled,
                Roles = u.Roles.Select(r => r.Nombre).ToList()
            };
        }

        public async Task<string> RegistrarUsuario(UsuarioCreateDTO u)
        {
            try
            {
                // Usamos el SP que maneja transacción de usuario + rol
                await _context.Database.ExecuteSqlRawAsync("EXEC usp_registrar_usuario @n, @e, @p, @r",
                    new SqlParameter("@n", u.Nombre),
                    new SqlParameter("@e", u.Email),
                    new SqlParameter("@p", u.Password), // En producción esto debería estar hasheado
                    new SqlParameter("@r", u.RolId));
                return "OK";
            }
            catch (Exception ex) { return ex.Message; }
        }

        public async Task<string> ActualizarUsuario(UsuarioUpdateDTO u)
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync("EXEC usp_actualizar_usuario @id, @n, @e, @en",
                    new SqlParameter("@id", u.UsuarioId),
                    new SqlParameter("@n", u.Nombre),
                    new SqlParameter("@e", u.Email),
                    new SqlParameter("@en", u.Enabled));
                return "OK";
            }
            catch (Exception ex) { return ex.Message; }
        }

        public async Task<string> DesactivarUsuario(long id)
        {
            try
            {
                // Hacemos una actualización simple a enabled = 0
                await _context.Database.ExecuteSqlRawAsync("UPDATE usuario SET enabled = 0 WHERE usuario_id = @id", new SqlParameter("@id", id));
                return "OK";
            }
            catch (Exception ex) { return ex.Message; }
        }
    }
}