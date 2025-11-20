namespace Proyecto_DSWI.Models
{
    public class Categoria
    {
        public short CategoriaId { get; set; }   // smallint
        public string Nombre { get; set; }       // varchar(80)
        public string Descripcion { get; set; } // varchar(255) NULL
        public bool Activo { get; set; }         // bit
        public DateTime UltimaActualizacion { get; set; } // datetime
    }
}
