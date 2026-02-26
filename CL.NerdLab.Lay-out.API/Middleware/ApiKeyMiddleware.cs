using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace CL.NerdLab.Lay_out.API.Middleware
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private const string APIKEY_HEADER_NAME = "X-Api-Key"; // El nombre del header que esperamos

        public ApiKeyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
        {
            // Solo aplicamos esto a las rutas de la API, dejamos Swagger y otras cosas libres por ahora (lo protegeremos distinto)
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                if (!context.Request.Headers.TryGetValue(APIKEY_HEADER_NAME, out var extractedApiKey))
                {
                    context.Response.StatusCode = 401; // Unauthorized
                    await context.Response.WriteAsync("Falta la API Key. Intento registrado.");
                    return;
                }

                var appSettingsApiKey = configuration.GetValue<string>("ApiSettings:ApiKey");

                if (!appSettingsApiKey.Equals(extractedApiKey))
                {
                    context.Response.StatusCode = 403; // Forbidden
                    await context.Response.WriteAsync("API Key Invalida. Intento registrado.");
                    return;
                }
            }

            await _next(context);
        }
    }
}