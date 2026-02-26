using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace CL.NerdLabLay_out.APP.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;

            // Configuramos la URL base y la API Key desde el appsettings.json
            _httpClient.BaseAddress = new System.Uri(_configuration["ApiConfig:BaseUrl"]);
            _httpClient.DefaultRequestHeaders.Add("X-Api-Key", _configuration["ApiConfig:ApiKey"]);
        }

        // Método genérico para hacer GET
        public async Task<T?> GetAsync<T>(string endpoint)
        {
            return await _httpClient.GetFromJsonAsync<T>(endpoint);
        }

        // Método genérico para hacer POST
        public async Task<HttpResponseMessage> PostAsync<T>(string endpoint, T data)
        {
            return await _httpClient.PostAsJsonAsync(endpoint, data);
        }

        // Puedes agregar métodos para PUT y DELETE si los necesitas después.
    }
}