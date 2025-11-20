namespace Proyecto_DSWI.Models
{

    public class Producto
    {
        public int ProductoId { get; set; }
        public string Sku { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int CategoriaId { get; set; }
        public string CodigoBarras { get; set; }
        public decimal PrecioLista { get; set; }
        public bool Activo { get; set; }
        public DateTime CreadoEn { get; set; }
        public DateTime UltimaActualizacion { get; set; }



    }
}
