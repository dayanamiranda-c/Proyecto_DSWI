using Proyecto_DSWI_API.DTOs; // Agregar using
using Proyecto_DSWI_API.Models;

namespace Proyecto_DSWI_API.Interfaces
{
    public interface IProducto
    {
        Task<IEnumerable<Producto>> ListarProductos();
        Task<Producto> ObtenerProducto(int id);

        // CAMBIO: Ahora reciben DTOs en lugar de la entidad completa
        Task<string> InsertarProducto(ProductoCreateDTO p);
        Task<string> ActualizarProducto(ProductoUpdateDTO p);
        Task<string> EliminarProducto(int id);
    }
}