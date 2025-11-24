using Microsoft.EntityFrameworkCore;
using Proyecto_DSWI.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("CadenaSQL");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));
var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    // He cambiado 'Home' por 'Catalogo' para que entres directo a tus productos
    pattern: "{controller=Catalogo}/{action=Inicio}/{id?}");

app.Run();