using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Proyecto_DSWI.Models;
using Microsoft.EntityFrameworkCore;

namespace Proyecto_DSWI.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // ----------------------- VISTA DE LOGIN (GET) -----------------------
        [HttpGet]
        public IActionResult Login()
        {
            // Devuelve la vista de login (Login.cshtml)
            return View();
        }

        // ----------------------- PROCESAR LOGIN (POST) -----------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 1. Buscar Usuario por Email e INCLUIR sus Roles
                var usuario = await _context.Usuarios
                    .Include(u => u.Roles) // ¡Crucial para cargar los roles M:M!
                    .FirstOrDefaultAsync(u => u.Email == model.Username); // El usuario ingresa su email

                // ... (Lógica de validación de contraseña - Asegúrate de usar el SIMULATED_HASH)
                bool passwordValida = (usuario != null) && (model.Password == usuario.PasswordHash); // <-- CAMBIAR POR LÓGICA DE HASHING REAL

                if (usuario != null && passwordValida)
                {
                    // 2. Crear Claims (Identidad)
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioId.ToString()),
                        new Claim(ClaimTypes.Name, usuario.Nombre!), // Nombre completo
                        new Claim(ClaimTypes.Email, usuario.Email!) // Email
                    };

                    // 3. Agregar Claims de Rol (iterando la colección de roles)
                    foreach (var rol in usuario.Roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, rol.Nombre!)); // Añadir el nombre del Rol ("Admin" o "User")
                    }

                    var claimsIdentity = new ClaimsIdentity(claims, "Cookies");

                    // 4. Iniciar Sesión
                    await HttpContext.SignInAsync("Cookies", new ClaimsPrincipal(claimsIdentity));

                    return RedirectToAction("Index", "Catalogo");
                }

                string mensajeError = $"Credenciales inválidas.";
                ModelState.AddModelError(string.Empty, mensajeError);
            }

            return View(model);
        }

        // ----------------------- CERRAR SESIÓN (LOGOUT) ----------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Elimina la cookie de autenticación (el Claim principal)
            await HttpContext.SignOutAsync("Cookies");

            // Redirige al usuario a la página de Login
            return RedirectToAction("Login", "Account");
        }

        // ----------------------- ACCESO DENEGADO (AccessDenied) -----------------------
        [HttpGet]
        public IActionResult AccessDenied()
        {
            // Obtiene el email del usuario logueado, si existe.
            ViewBag.Email = User.Identity?.Name;

            // Aquí puedes capturar el email del usuario logueado si lo deseas
            return View();
        }
    }
}