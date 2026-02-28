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
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserRole"))) return RedirectToPage("/Login/Login");
            return Page();
        }

        public async Task<IActionResult> OnGetFlotaAsync()
        {
            try { return new JsonResult(await _apiService.GetAsync<object>("RegistroActividadBuses/EstadoFlota") ?? new List<object>()); }
            catch { return new JsonResult(new List<object>()); }
        }

        [HttpPost]
        public async Task<IActionResult> OnPostCambiarEstadoAsync([FromBody] CambioEstadoRequest request)
        {
            request.RutUsuario = HttpContext.Session.GetString("UserRut") ?? "";
            var response = await _apiService.PostAsync("RegistroActividadBuses/CambiarEstado", request);
            if (response.IsSuccessStatusCode) return new JsonResult(new { success = true });
            return new JsonResult(new { success = false, message = "Error en la API." });
        }

        // --- NUEVO: PUENTE PARA ASIGNAR RECORRIDO ---
        [HttpPost]
        public async Task<IActionResult> OnPostAsignarRecorridoAsync([FromBody] AsignarRecorridoRequest request)
        {
            request.RutUsuario = HttpContext.Session.GetString("UserRut") ?? "";
            var response = await _apiService.PostAsync("RegistroActividadBuses/AsignarRecorrido", request);
            if (response.IsSuccessStatusCode) return new JsonResult(new { success = true });
            return new JsonResult(new { success = false, message = "Error al asignar recorrido en la API." });
        }

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

        public async Task<IActionResult> OnGetHistorialGlobalAsync(string filtro = "Hoy", string? inicio = null, string? fin = null)
        {
            try
            {
                string endpoint = "RegistroActividadBuses/HistorialCarga/" + (filtro == "Rango" && !string.IsNullOrEmpty(inicio) ? $"Rango?inicio={inicio}&fin={fin}" : filtro == "Reciente" ? "Reciente" : "Hoy");
                return new JsonResult(await _apiService.GetAsync<object>(endpoint) ?? new List<object>());
            }
            catch { return new JsonResult(new List<object>()); }
        }

        public async Task<IActionResult> OnGetUsuariosAsync() { return new JsonResult(await _apiService.GetAsync<object>("Usuarios/Detalle") ?? new List<object>()); }
        public async Task<IActionResult> OnGetRolesAsync() { return new JsonResult(await _apiService.GetAsync<object>("Roles") ?? new List<object>()); }

        [HttpPost]
        public async Task<IActionResult> OnPostCrearUsuarioAsync([FromBody] NuevoUsuarioRequest request)
        {
            request.PasswordTemp = "1234"; request.PasswordHash = "-"; request.Activo = true;
            var response = await _apiService.PostAsync("Usuarios", request);
            return new JsonResult(new { success = response.IsSuccessStatusCode });
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

        // --- NUEVO: PUENTE PARA EL LAYOUT DEL PATIO ---
        public async Task<IActionResult> OnGetPatioLayoutAsync(int idPatio = 1)
        {
            try
            {
                return new JsonResult(await _apiService.GetAsync<object>($"PatioLayout/{idPatio}") ?? new List<object>());
            }
            catch
            {
                return new JsonResult(new List<object>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> OnPostGuardarZonaAsync([FromBody] DtoGuardarZona request)
        {
            var response = await _apiService.PostAsync("PatioLayout/GuardarZona", request);
            return new JsonResult(new { success = response.IsSuccessStatusCode });
        }

        // 4. Reemplaza OnPostAsignarBusSlotAsync por este (Le ponemos el try/catch que le faltaba):
        [HttpPost]
        public async Task<IActionResult> OnPostAsignarBusSlotAsync([FromBody] DtoAsignarBus request)
        {
            try
            {
                var response = await _apiService.PostAsync("PatioLayout/AsignarBus", request);
                return new JsonResult(new { success = response.IsSuccessStatusCode });
            }
            catch { return new JsonResult(new { success = false, message = "Error de red" }); }
        }

        [HttpPost]
        public async Task<IActionResult> OnPostQuitarBusSlotAsync([FromBody] int idSlot)
        {
            var response = await _apiService.PostAsync("PatioLayout/QuitarBus", idSlot);
            return new JsonResult(new { success = response.IsSuccessStatusCode });
        }


        // 2. Reemplaza OnPostRotarZonaAsync por este:
        [HttpPost]
        public async Task<IActionResult> OnPostRotarZonaAsync([FromBody] DtoIdRequest request)
        {
            try
            {
                var response = await _apiService.PostAsync($"PatioLayout/RotarZona/{request.Id}", new { });
                return new JsonResult(new { success = response.IsSuccessStatusCode });
            }
            catch { return new JsonResult(new { success = false, message = "Error de red" }); }
        }

        // 3. Reemplaza OnPostQuitarBusSlotAsync por este:
        [HttpPost]
        public async Task<IActionResult> OnPostQuitarBusSlotAsync([FromBody] DtoIdRequest request)
        {
            try
            {
                var response = await _apiService.PostAsync("PatioLayout/QuitarBus", request.Id);
                return new JsonResult(new { success = response.IsSuccessStatusCode });
            }
            catch { return new JsonResult(new { success = false, message = "Error de red" }); }
        }

        // 1. Agrega esta mini-clase al final de Index.cshtml.cs junto a las otras
        public class DtoIdRequest { public int Id { get; set; } }

        // Reemplaza tu clase espejo DtoGuardarZona actual por esta:
        public class DtoGuardarZona { public int IdZona { get; set; } public int IdPatio { get; set; } public string NombreZona { get; set; } public string ColorHex { get; set; } public int Filas { get; set; } public int Columnas { get; set; } public string Orientacion { get; set; } }
        public class DtoAsignarBus
        {
            public int IdSlot { get; set; }
            public string Patente { get; set; } // <--- CAMBIO CLAVE: Ahora es string
            public int IdTipoVehiculo { get; set; }
        }

        public class EditarUsuarioRequest { public int IdUsuario { get; set; } public string NombreCompleto { get; set; } public string Rut { get; set; } public string Email { get; set; } public int IdRol { get; set; } }
        public class CambioEstadoRequest { public string Patente { get; set; } public string Estado { get; set; } public int? Porcentaje { get; set; } public string? NumeroRecorrido { get; set; } public string? RutUsuario { get; set; } }
        public class AsignarRecorridoRequest { public string Patente { get; set; } public string NumeroRecorrido { get; set; } public string? RutUsuario { get; set; } }
        public class NuevoUsuarioRequest { public string NombreCompleto { get; set; } public string Rut { get; set; } public string Email { get; set; } public int IdRol { get; set; } public string? PasswordTemp { get; set; } public string? PasswordHash { get; set; } public bool Activo { get; set; } }
    }
}