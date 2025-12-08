using Microsoft.EntityFrameworkCore;

namespace Proyecto_DSWI.Models // Lo ponemos aquí para que no necesites carpetas extra
{
    // Esta clase es el "Puente" obligatorio para conectar tu SQL con C#
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        // Aquí le decimos al programa: "Oye, existen estas tablas en mi BD"        // ... (otros DbSets)

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Almacen> Almacenes { get; set; }
        public DbSet<Inventario> Inventarios { get; set; }
        public DbSet<InventarioMovimiento> InventarioMovimientos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuraciones existentes
            modelBuilder.Entity<Inventario>()
                .HasKey(i => new { i.AlmacenId, i.ProductoId });
            // ------------------------------------------------------------------
            // CONFIGURACIÓN OBLIGATORIA DE LA RELACIÓN M:M
            // Esto le dice a EF Core que use la tabla existente 'usuario_roles'
            // ------------------------------------------------------------------
            modelBuilder.Entity<Usuario>()
                .HasMany(u => u.Roles)
                .WithMany(r => r.Usuarios)
                .UsingEntity<Dictionary<string, object>>(
                    "usuario_roles", // <--- Nombre EXACTO de la tabla intermedia
                    j => j.HasOne<Rol>().WithMany().HasForeignKey("rol_id"), // Clave foránea a Rol
                    j => j.HasOne<Usuario>().WithMany().HasForeignKey("usuario_id") // Clave foránea a Usuario
                )
                // Reforzamos el nombre de la tabla de unión para evitar el error 'RolUsuario'
                .ToTable("usuario_roles")
                // También especificamos la clave compuesta de la tabla de unión
                .HasKey("usuario_id", "rol_id");
            // ------------------------------------------------------------------
        }
    }
}