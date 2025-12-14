using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto_DSWI_API.Data;
using Proyecto_DSWI_API.DTOs;

namespace Proyecto_DSWI_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventarioController : ControllerBase
    {
        private readonly AppDbContext _context;

        public InventarioController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int? almacenId)
        {
            // Consultamos relacionando las tablas
            var query = _context.Inventarios
                .Include(i => i.Producto)
                .Include(i => i.Almacen)
                .AsQueryable();

            // Filtro opcional si mandan ID de almacen
            if (almacenId.HasValue && almacenId.Value > 0)
            {
                query = query.Where(i => i.AlmacenId == almacenId);
            }

            // Convertimos a DTO
            var resultado = await query
                .Select(i => new InventarioDTO
                {
                    Producto = i.Producto.Nombre,
                    Sku = i.Producto.Sku,
                    Almacen = i.Almacen.Nombre,
                    StockActual = (int)i.Cantidad,
                    StockMinimo = (int)i.StockMin,
                    Precio = i.Producto.PrecioLista ?? 0
                })
                .ToListAsync();

            return Ok(resultado);
        }
    }
}