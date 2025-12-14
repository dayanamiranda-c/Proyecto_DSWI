using Proyecto_DSWI_API.DTOs;
using Proyecto_DSWI_API.Models;

namespace Proyecto_DSWI_API.Interfaces
{
    public interface ICategoria
    {
        Task<IEnumerable<Categoria>> ListarCategorias();
        Task<Categoria> ObtenerCategoria(int id);
        Task<string> InsertarCategoria(CategoriaCreateDTO c);
        Task<string> ActualizarCategoria(CategoriaUpdateDTO c);
        Task<string> EliminarCategoria(int id);
    }
}