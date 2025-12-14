using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Proyecto_DSWI_API.Data;
using Proyecto_DSWI_API.DTOs;
using Proyecto_DSWI_API.Interfaces;
using Proyecto_DSWI_API.Models;

namespace Proyecto_DSWI_API.Repositorio
{
    public class CategoriaDAO : ICategoria
    {
        private readonly AppDbContext _context;
        public CategoriaDAO(AppDbContext context) { _context = context; }

        public async Task<IEnumerable<Categoria>> ListarCategorias()
        {
            return await _context.Categorias.Where(c => c.Activo).ToListAsync();
        }

        public async Task<Categoria> ObtenerCategoria(int id)
        {
            return await _context.Categorias.FindAsync((short)id);
        }

        public async Task<string> InsertarCategoria(CategoriaCreateDTO c)
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync("EXEC usp_insertar_categoria @n, @d",
                    new SqlParameter("@n", c.Nombre), new SqlParameter("@d", c.Descripcion ?? ""));
                return "OK";
            }
            catch (Exception ex) { return ex.Message; }
        }

        public async Task<string> ActualizarCategoria(CategoriaUpdateDTO c)
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync("EXEC usp_actualizar_categoria @id, @n, @d",
                    new SqlParameter("@id", c.CategoriaId), new SqlParameter("@n", c.Nombre), new SqlParameter("@d", c.Descripcion ?? ""));
                return "OK";
            }
            catch (Exception ex) { return ex.Message; }
        }

        public async Task<string> EliminarCategoria(int id)
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync("EXEC usp_eliminar_categoria @id", new SqlParameter("@id", id));
                return "OK";
            }
            catch (Exception ex) { return ex.Message; }
        }
    }
}