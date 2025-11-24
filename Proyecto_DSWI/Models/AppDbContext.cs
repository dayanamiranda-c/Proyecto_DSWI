using Microsoft.EntityFrameworkCore;

namespace Proyecto_DSWI.Models // Lo ponemos aquí para que no necesites carpetas extra
{
    // Esta clase es el "Puente" obligatorio para conectar tu SQL con C#
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Aquí le decimos al programa: "Oye, existen estas tablas en mi BD"
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Almacen> Almacenes { get; set; }
        public DbSet<Inventario> Inventarios { get; set; }
        public DbSet<InventarioMovimiento> InventarioMovimientos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Esto es necesario para la tabla Inventario que tiene llave compuesta
            modelBuilder.Entity<Inventario>()
                .HasKey(i => new { i.AlmacenId, i.ProductoId });
        }
    }
}