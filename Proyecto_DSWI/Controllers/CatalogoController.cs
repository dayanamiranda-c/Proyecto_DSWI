using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Proyecto_DSWI.Models;
using System;

namespace Proyecto_DSWI.Controllers
{
    public class CatalogoController : Controller
    {
        private readonly AppDbContext _context;

        public CatalogoController(AppDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public IActionResult Inicio()
        {

            return View();
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var productos = await _context.Productos
                .Include(p => p.Categoria) // Para ver el nombre de la categoría
                .ToListAsync();

            return View(productos);
        }

  
        [HttpGet]
        public IActionResult Nuevo()
        {
            // Carga el combo de categorías
            ViewBag.Categorias = new SelectList(_context.Categorias, "CategoriaId", "Nombre");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Nuevo(Producto producto)
        {
            if (ModelState.IsValid)
            {
                producto.CreadoEn = DateTime.Now;
                producto.UltimaActualizacion = DateTime.Now;
                producto.Activo = true;

                _context.Add(producto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

   
            ViewBag.Categorias = new SelectList(_context.Categorias, "CategoriaId", "Nombre");
            return View(producto);
        }

        [HttpGet]
        public async Task<IActionResult> Movimientos()
        {
            var historial = await _context.InventarioMovimientos
                .Include(m => m.Almacen)
                .Include(m => m.Producto)
                .OrderByDescending(m => m.FechaMovimiento)
                .ToListAsync();

            return View(historial);
        }

 
        [HttpGet]
        public async Task<IActionResult> Inventario(short? id)
        {
            // Consulta base
            var query = _context.Inventarios
                .Include(i => i.Almacen)
                .Include(i => i.Producto)
                .AsQueryable();


            if (id.HasValue)
            {
                query = query.Where(i => i.AlmacenId == id);
            }

            // Cargamos la lista para el filtro en la vista
            ViewBag.Almacenes = new SelectList(_context.Almacenes, "AlmacenId", "Nombre");

            return View(await query.ToListAsync());
        }
    }
}