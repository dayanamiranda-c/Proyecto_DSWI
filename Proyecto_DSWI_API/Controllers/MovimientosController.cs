using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto_DSWI_API.Data;
using Proyecto_DSWI_API.DTOs;

namespace Proyecto_DSWI_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovimientosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MovimientosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var historial = await _context.InventarioMovimientos
                .Include(m => m.Producto)
                .Include(m => m.Almacen)
                .OrderByDescending(m => m.FechaMovimiento)
                .Select(m => new MovimientoDTO
                {
                    // CORRECCIÓN 1: Usamos el nombre real de tu modelo
                    MovimientoId = m.InventarioMovimientoId,

                    Fecha = m.FechaMovimiento,

                    // Validamos nulos con ?? por seguridad
                    Producto = m.Producto != null ? m.Producto.Nombre : "Desconocido",
                    Sku = m.Producto != null ? m.Producto.Sku : "-",
                    Almacen = m.Almacen != null ? m.Almacen.Nombre : "-",

                    TipoMovimiento = m.TipoMovimiento,
                    Cantidad = m.Cantidad,

                    // CORRECCIÓN 2: Usamos tu columna string 'Usuario'
                    Usuario = m.Usuario ?? "Sistema"
                })
                .ToListAsync();

            return Ok(historial);
        }
    }
}