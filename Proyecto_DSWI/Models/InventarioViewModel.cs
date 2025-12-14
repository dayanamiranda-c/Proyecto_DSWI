namespace Proyecto_DSWI.Models
{
    public class InventarioViewModel
    {
        public string Producto { get; set; }
        public string Sku { get; set; }
        public string Almacen { get; set; }
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public decimal Precio { get; set; }
        public decimal ValorTotal { get; set; }
    }
}