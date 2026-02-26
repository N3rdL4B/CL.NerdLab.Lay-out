using CL.NerdLabLay_out.APP.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CL.NerdLabLay_out.APP.Pages.Login
{
    public class LoginModel : PageModel
    {
        //TEST
        private readonly ApiService _apiService;

        public LoginModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        public void OnGet()
        {
        }

        [HttpPost]
        public async Task<IActionResult> OnPostAuthenticate([FromBody] LoginAuthRequest request)
        {
            try
            {
                // Llamada a la API real
                var response = await _apiService.PostAsync("Auth/Login", request);
                var data = await response.Content.ReadFromJsonAsync<ApiAuthResponse>();

                if (response.IsSuccessStatusCode && data != null && data.Success)
                {
                    // Si la API dice que hay que cambiar clave, le avisamos al JS
                    if (data.RequirePasswordChange)
                    {
                        return new JsonResult(new { success = true, requirePasswordChange = true });
                    }

                    // Si está todo OK, creamos la sesión con los datos reales de la BD
                    HttpContext.Session.SetString("UserRole", data.Role ?? "Desconocido");
                    HttpContext.Session.SetString("UserName", data.Nombre ?? "Usuario");
                    HttpContext.Session.SetString("UserRut", data.Rut ?? ""); // <--- ¡NUEVO!

                    return new JsonResult(new { success = true, requirePasswordChange = false, redirectUrl = "/Index" });
                }

                // Si la API devuelve un BadRequest o credenciales inválidas
                return new JsonResult(new { success = false, message = data?.Message ?? "RUT o contraseña incorrectos." });
            }
            catch (Exception ex)
            {
                // Por si la API está caída
                return new JsonResult(new { success = false, message = "Error de comunicación con el servidor central." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> OnPostChangePassword([FromBody] ChangePassRequest request)
        {
            try
            {
                // Llamada a la API real para procesar el cambio de clave
                var response = await _apiService.PostAsync("Auth/ChangePassword", request);
                var data = await response.Content.ReadFromJsonAsync<ApiAuthResponse>();

                if (response.IsSuccessStatusCode && data != null && data.Success)
                {
                    // Clave cambiada exitosamente, le damos acceso de inmediato
                    HttpContext.Session.SetString("UserRole", data.Role ?? "Desconocido");
                    HttpContext.Session.SetString("UserName", data.Nombre ?? "Usuario");

                    return new JsonResult(new { success = true, redirectUrl = "/Index" });
                }

                return new JsonResult(new { success = false, message = data?.Message ?? "No se pudo actualizar la contraseña. Verifique sus datos." });
            }
            catch (Exception)
            {
                return new JsonResult(new { success = false, message = "Error de comunicación con el servidor central." });
            }
        }

        // --- DTOs (Modelos de datos) ---
        public class LoginAuthRequest { public string Rut { get; set; } public string Password { get; set; } }

        public class ChangePassRequest { public string Rut { get; set; } public string CurrentPassword { get; set; } public string NewPassword { get; set; } }

        // DTO para leer lo que nos responde la API de AuthController
        public class ApiAuthResponse
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
            public bool RequirePasswordChange { get; set; }
            public string? Role { get; set; }
            public string? Nombre { get; set; }
            public string? Rut { get; set; } // <--- ¡NUEVO!
        }
    }
}