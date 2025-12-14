namespace Proyecto_DSWI_API.DTOs
{
    public class CategoriaCreateDTO
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
    }

    public class CategoriaUpdateDTO
    {
        public short CategoriaId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
    }
}