using Microsoft.EntityFrameworkCore;
using Proyecto_DSWI_API.Data;
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
            // Para listados simples podemos usar EF Core directo
            return await _context.Productos.Include(p => p.Categoria).ToListAsync();
        }

        public async Task<Producto> ObtenerProducto(int id)
        {
            return await _context.Productos.FindAsync(id);
        }

        public async Task<string> InsertarProducto(Producto p)
        {
            try
            {
                // REQUISITO V: Mantenimiento usando Store Procedures
                // Asume que tienes un SP llamado 'usp_insertar_producto' en SQL Server
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC usp_insertar_producto @nombre, @sku, @categoria, @precio, @stock",
                    new Microsoft.Data.SqlClient.SqlParameter("@nombre", p.Nombre),
                    new Microsoft.Data.SqlClient.SqlParameter("@sku", p.Sku),
                    new Microsoft.Data.SqlClient.SqlParameter("@categoria", p.CategoriaId),
                    new Microsoft.Data.SqlClient.SqlParameter("@precio", p.PrecioLista),
                    // Asumiendo que guardas stock en inventario o producto, ajusta según tu BD
                    new Microsoft.Data.SqlClient.SqlParameter("@stock", 0)
                );
                return "Producto registrado correctamente";
            }
            catch (Exception ex)
            {
                return "Error al registrar: " + ex.Message;
            }
        }
    }
}
