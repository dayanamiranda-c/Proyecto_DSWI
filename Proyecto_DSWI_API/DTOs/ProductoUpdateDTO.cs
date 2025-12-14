namespace Proyecto_DSWI_API.DTOs
{
    public class ProductoUpdateDTO
    {
        public int ProductoId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public short CategoriaId { get; set; }
        public decimal Precio { get; set; }
    }
}