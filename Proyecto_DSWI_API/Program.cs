//using Microsoft.EntityFrameworkCore;
using Proyecto_DSWI_API.Models;
// AGREGAR ESTAS LÍNEAS DE USING:
using System.Text.Json.Serialization;
using Proyecto_DSWI_API.Data;
using Proyecto_DSWI_API.Interfaces;
using Proyecto_DSWI_API.Repositorio;

var builder = WebApplication.CreateBuilder(args);

// Configuración de la BD (que ya tenías)
var connectionString = builder.Configuration.GetConnectionString("CadenaSQL");
//builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

// --- REGISTRO DE REPOSITORIOS (INYECCIÓN DE DEPENDENCIAS) ---

builder.Services.AddScoped<IProducto, ProductoDAO>();
//builder.Services.AddScoped<IVentas, VentasDAO>();

// -------------------------------------------------------------

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ... resto del archivo (Swagger, Https, MapControllers, Run) ...
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();