namespace Proyecto_DSWI_API.DTOs
{
    public class MovimientoDTO
    {
        public long MovimientoId { get; set; } // Lo dejaremos así para el JSON
        public DateTime Fecha { get; set; }
        public string Producto { get; set; }
        public string Sku { get; set; }
        public string Almacen { get; set; }
        public string TipoMovimiento { get; set; }
        public decimal Cantidad { get; set; } // CAMBIO: int -> decimal
        public string Usuario { get; set; }
    }
}