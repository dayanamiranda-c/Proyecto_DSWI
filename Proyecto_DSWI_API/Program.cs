using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Proyecto_DSWI_API.Data;
using Proyecto_DSWI_API.Interfaces;
using Proyecto_DSWI_API.Repositorio;

var builder = WebApplication.CreateBuilder(args);

// =======================================================================
// 1. CONFIGURAR LA BASE DE DATOS (Esto lo borraste y es vital)
// =======================================================================
// Busca la cadena de conexión llamada "sql" en appsettings.json
var connectionString = builder.Configuration.GetConnectionString("sql");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// =======================================================================
// 2. INYECCIÓN DE DEPENDENCIAS (Esto es lo que causa tu error 500)
// =======================================================================
// Aquí le dices: "Cuando el Controller pida IProducto, dale ProductoDAO"
builder.Services.AddScoped<IProducto, ProductoDAO>();
builder.Services.AddScoped<IVentas, VentasDAO>();
builder.Services.AddScoped<ICategoria, CategoriaDAO>();
builder.Services.AddScoped<IUsuario, UsuarioDAO>();
// =======================================================================
// 3. RESTO DE SERVICIOS
// =======================================================================
builder.Services.AddControllers().AddJsonOptions(options =>
{
    // Esto evita el error del ciclo infinito "Categoria -> Producto -> Categoria..."
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
}); 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();