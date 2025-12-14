namespace Proyecto_DSWI.Models
{
    public class MovimientoViewModel
    {
        public long MovimientoId { get; set; }
        public DateTime Fecha { get; set; }
        public string Producto { get; set; }
        public string Sku { get; set; }
        public string Almacen { get; set; }
        public string TipoMovimiento { get; set; }
        public decimal Cantidad { get; set; } // CAMBIO: int -> decimal
        public string Usuario { get; set; }
    }
}