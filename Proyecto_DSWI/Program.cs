using Microsoft.EntityFrameworkCore;
using Proyecto_DSWI.Models;

var builder = WebApplication.CreateBuilder(args);
// ... (código anterior)

// CONFIGURACIÓN DEL CLIENTE HTTP (Para consumir la API)
var apiUrl = builder.Configuration["ApiSettings:BaseUrl"];
builder.Services.AddHttpClient("TiendaAPI", client =>
{
    client.BaseAddress = new Uri(apiUrl!);
});

// ... (Resto de tu código: Session, Auth, Controllers, etc.)
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication("Cookies") // Especifica el esquema de autenticación por defecto (Cookies)
    .AddCookie("Cookies", options =>
    {
        // Ruta a la que se redirigirá si el usuario intenta acceder a una vista protegida sin iniciar sesión
        options.LoginPath = "/Account/Login";

        // (Opcional) Ruta de acceso denegado si el usuario no tiene el rol necesario
        options.AccessDeniedPath = "/Account/AccessDenied";

        // (Opcional) Nombre de la cookie
        options.Cookie.Name = "MyAppAuthCookie";
    });

// 2. Añadir el servicio de Autorización
builder.Services.AddAuthorization(options =>
{
    // Opcional: Puedes definir políticas de autorización más complejas aquí.
    // Para roles simples, el atributo [Authorize(Roles="...")] es suficiente.
});

// Agregar soporte para sesión
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // El carrito expira en 30 mins
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();

app.UseRouting();

app.UseSession(); // <--- IMPORTANTE: Activar el middleware

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    // He cambiado 'Home' por 'Catalogo' para que entres directo a tus productos
    pattern: "{controller=Catalogo}/{action=Inicio}/{id?}");

app.Run();