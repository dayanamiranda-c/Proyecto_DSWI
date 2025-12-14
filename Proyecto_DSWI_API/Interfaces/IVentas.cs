using Proyecto_DSWI_API.DTOs;

namespace Proyecto_DSWI_API.Interfaces
{
    public interface IVentas
    {
        Task<string> RegistrarVenta(VentaDTO venta);
    }
}
