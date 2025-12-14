using Proyecto_DSWI_API.Models;

namespace Proyecto_DSWI_API.Interfaces
{
    public interface IProducto
    {
        Task<IEnumerable<Producto>> ListarProductos();
        Task<Producto> ObtenerProducto(int id);
        Task<string> InsertarProducto(Producto producto);
    }
}
