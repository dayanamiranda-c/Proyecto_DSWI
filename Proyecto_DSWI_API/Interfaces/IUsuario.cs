using Proyecto_DSWI_API.DTOs;

namespace Proyecto_DSWI_API.Interfaces
{
    public interface IUsuario
    {
        Task<IEnumerable<UsuarioResponseDTO>> ListarUsuarios();
        Task<UsuarioResponseDTO> ObtenerUsuario(long id);
        Task<string> RegistrarUsuario(UsuarioCreateDTO u);
        Task<string> ActualizarUsuario(UsuarioUpdateDTO u);
        // El delete puede ser físico o lógico (desactivar)
        Task<string> DesactivarUsuario(long id);
    }
}