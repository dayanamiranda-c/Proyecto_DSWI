namespace Proyecto_DSWI_API.DTOs
{
    // Para ver usuarios (sin password)
    public class UsuarioResponseDTO
    {
        public long UsuarioId { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public bool Enabled { get; set; }
        public List<string> Roles { get; set; }
    }

    // Para crear (con password y rol)
    public class UsuarioCreateDTO
    {
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public long RolId { get; set; } // 1: Admin, 2: User (según tu BD)
    }

    // Para editar (sin password, eso suele ir aparte)
    public class UsuarioUpdateDTO
    {
        public long UsuarioId { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public bool Enabled { get; set; }
    }
}