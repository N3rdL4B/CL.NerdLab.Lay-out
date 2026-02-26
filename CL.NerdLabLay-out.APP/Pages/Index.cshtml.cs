using CL.NerdLabLay_out.APP.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CL.NerdLabLay_out.APP.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApiService _apiService;

        public IndexModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        public IActionResult OnGet()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserRole")))
                return RedirectToPage("/Login/Login");
            return Page();
        }

        public async Task<IActionResult> OnGetFlotaAsync()
        {
            try
            {
                // ¡AQUÍ ESTABA EL DETALLE! Le quitamos el "Get" a la ruta para que coincida con la API
                var buses = await _apiService.GetAsync<object>("RegistroActividadBuses/EstadoFlota");
                return new JsonResult(buses ?? new List<object>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando flota: {ex.Message}");
                return new JsonResult(new { success = false, message = "No se pudo cargar la flota", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> OnPostCambiarEstadoAsync([FromBody] CambioEstadoRequest request)
        {
            request.RutUsuario = HttpContext.Session.GetString("UserRut") ?? "";
            var response = await _apiService.PostAsync("RegistroActividadBuses/CambiarEstado", request);
            if (response.IsSuccessStatusCode) return new JsonResult(new { success = true });
            return new JsonResult(new { success = false, message = "Error al actualizar estado en la API." });
        }

        public async Task<IActionResult> OnGetHistorialGlobalAsync(string filtro = "Hoy", string? inicio = null, string? fin = null)
        {
            try
            {
                string endpoint = "RegistroActividadBuses/HistorialCarga/";

                if (filtro == "Rango" && !string.IsNullOrEmpty(inicio) && !string.IsNullOrEmpty(fin))
                    endpoint += $"Rango?inicio={inicio}&fin={fin}";
                else if (filtro == "Reciente")
                    endpoint += "Reciente";
                else
                    endpoint += "Hoy";

                var historial = await _apiService.GetAsync<object>(endpoint);
                return new JsonResult(historial ?? new List<object>());
            }
            catch (Exception)
            {
                return new JsonResult(new List<object>());
            }
        }

        public async Task<IActionResult> OnGetUsuariosAsync()
        {
            var users = await _apiService.GetAsync<object>("Usuarios/Detalle");
            return new JsonResult(users ?? new List<object>());
        }

        public async Task<IActionResult> OnGetRolesAsync()
        {
            var roles = await _apiService.GetAsync<object>("Roles");
            return new JsonResult(roles ?? new List<object>());
        }

        [HttpPost]
        public async Task<IActionResult> OnPostCrearUsuarioAsync([FromBody] NuevoUsuarioRequest request)
        {
            request.PasswordTemp = "1234";
            request.PasswordHash = "-";
            request.Activo = true;
            var response = await _apiService.PostAsync("Usuarios", request);
            if (response.IsSuccessStatusCode) return new JsonResult(new { success = true });
            return new JsonResult(new { success = false, message = "Error al crear usuario." });
        }

        [HttpPost]
        public async Task<IActionResult> OnPostToggleUsuarioAsync([FromBody] int id)
        {
            var response = await _apiService.PostAsync($"Usuarios/ToggleActivo/{id}", new { });
            return new JsonResult(new { success = response.IsSuccessStatusCode });
        }

        [HttpPost]
        public async Task<IActionResult> OnPostResetPasswordAsync([FromBody] int id)
        {
            var response = await _apiService.PostAsync($"Usuarios/ResetPassword/{id}", new { });
            return new JsonResult(new { success = response.IsSuccessStatusCode });
        }

        // --- PUENTES NUEVOS ---
        public async Task<IActionResult> OnGetPatentesAsync()
        {
            try { return new JsonResult(await _apiService.GetAsync<List<string>>("RegistroActividadBuses/Patentes") ?? new List<string>()); }
            catch { return new JsonResult(new List<string>()); }
        }

        public async Task<IActionResult> OnGetHistorialBusAsync(string patente)
        {
            try { return new JsonResult(await _apiService.GetAsync<object>($"RegistroActividadBuses/HistorialCarga/Bus/{patente}") ?? new List<object>()); }
            catch { return new JsonResult(new List<object>()); }
        }

        [HttpPost]
        public async Task<IActionResult> OnPostEditarUsuarioAsync([FromBody] EditarUsuarioRequest request)
        {
            try
            {
                var response = await _apiService.PostAsync($"Usuarios/Editar/{request.IdUsuario}", request);
                return new JsonResult(new { success = response.IsSuccessStatusCode });
            }
            catch { return new JsonResult(new { success = false, message = "Error de red" }); }
        }

        public class EditarUsuarioRequest { public int IdUsuario { get; set; } public string NombreCompleto { get; set; } public string Rut { get; set; } public string Email { get; set; } public int IdRol { get; set; } }
        public class CambioEstadoRequest { public string Patente { get; set; } public string Estado { get; set; } public int? Porcentaje { get; set; } public string? RutUsuario { get; set; } }
        public class NuevoUsuarioRequest { public string NombreCompleto { get; set; } public string Rut { get; set; } public string Email { get; set; } public int IdRol { get; set; } public string? PasswordTemp { get; set; } public string? PasswordHash { get; set; } public bool Activo { get; set; } }
    }
}