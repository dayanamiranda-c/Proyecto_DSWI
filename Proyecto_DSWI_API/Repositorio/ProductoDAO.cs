using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient; // Necesario para SqlParameter
using Proyecto_DSWI_API.Data;
using Proyecto_DSWI_API.DTOs;
using Proyecto_DSWI_API.Interfaces;
using Proyecto_DSWI_API.Models;

namespace Proyecto_DSWI_API.Repositorio
{
    public class ProductoDAO : IProducto
    {
        private readonly AppDbContext _context;

        public ProductoDAO(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Producto>> ListarProductos()
        {
            return await _context.Productos
                .Include(p => p.Categoria)
                .Where(p => p.Activo == true) // Solo mostrar activos
                .ToListAsync();
        }

        public async Task<Producto> ObtenerProducto(int id)
        {
            return await _context.Productos.FindAsync(id);
        }

        public async Task<string> InsertarProducto(ProductoCreateDTO p)
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC usp_insertar_producto @nombre, @sku, @categoria, @precio, @stock",
                    new SqlParameter("@nombre", p.Nombre),
                    new SqlParameter("@sku", p.Sku),
                    new SqlParameter("@categoria", p.CategoriaId),
                    new SqlParameter("@precio", p.Precio),
                    new SqlParameter("@stock", p.StockInicial)
                );
                return "OK";
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }

        public async Task<string> ActualizarProducto(ProductoUpdateDTO p)
        {
            try
            {
                // Verifica si existe antes de intentar actualizar
                var existe = await _context.Productos.AnyAsync(x => x.ProductoId == p.ProductoId);
                if (!existe) return "Producto no encontrado";

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC usp_actualizar_producto @id, @nombre, @categoria, @precio, @descripcion",
                    new SqlParameter("@id", p.ProductoId),
                    new SqlParameter("@nombre", p.Nombre),
                    new SqlParameter("@categoria", p.CategoriaId),
                    new SqlParameter("@precio", p.Precio),
                    new SqlParameter("@descripcion", p.Descripcion ?? "")
                );
                return "OK";
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }

        public async Task<string> EliminarProducto(int id)
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC usp_eliminar_producto @id",
                    new SqlParameter("@id", id)
                );
                return "OK";
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }
    }
}