using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Proyecto_DSWI.Models;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Proyecto_DSWI.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory; // Inyectamos HTTP, no DB
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // 1. Preparar datos para enviar a la API
            var loginData = new { Email = model.Username, Password = model.Password };
            var jsonContent = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");

            // 2. Llamar a la API de Auth
            var client = _httpClientFactory.CreateClient("TiendaAPI");
            var response = await client.PostAsync("api/auth/login", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                // 3. Leer respuesta (Usuario + Roles)
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var authUser = JsonSerializer.Deserialize<AuthResponse>(jsonResponse, options);

                // 4. Crear la Cookie de Sesión en el MVC
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, authUser.UsuarioId.ToString()),
                    new Claim(ClaimTypes.Name, authUser.Nombre),
                    new Claim(ClaimTypes.Email, authUser.Email)
                };

                // Agregar roles
                if (authUser.Roles != null)
                {
                    foreach (var rol in authUser.Roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, rol));
                    }
                }

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Index", "Catalogo");
            }

            ModelState.AddModelError("", "Credenciales inválidas (API).");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // DTO Auxiliar para leer la respuesta JSON
        public class AuthResponse
        {
            public long UsuarioId { get; set; }
            public string Nombre { get; set; }
            public string Email { get; set; }
            public List<string> Roles { get; set; }
        }
    }
}