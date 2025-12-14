namespace Proyecto_DSWI_API.DTOs
{
    public class DetalleVentaDTO
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
        public decimal Subtotal { get; set; }
    }
}
