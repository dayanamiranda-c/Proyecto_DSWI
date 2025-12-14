namespace Proyecto_DSWI_API.DTOs
{
    public class VentaDTO
    {
        public long UsuarioId { get; set; }
        public decimal Total { get; set; }
        public List<DetalleVentaDTO> Detalles { get; set; } = new List<DetalleVentaDTO>();
    }
}
