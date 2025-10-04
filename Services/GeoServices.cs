using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ServiceLocator.Services
{
    public class GeoServices
    {
        private readonly string? _apiKey;
        private readonly HttpClient _httpClient;

        public GeoServices(IConfiguration configuration)
        {
            _apiKey = configuration["Positionstack:ApiKey"];
            _httpClient = new HttpClient();
        }

        // This method takes a ZIP code and returns (latitude, longitude) or null if not found
        public async Task<(double Latitude, double Longitude)?> GetCoordinatesAsync(string zipCode)
        {
            // Construct the API URL with your API key and zip code query
            var url = $"http://api.positionstack.com/v1/forward?access_key={_apiKey}&query={Uri.EscapeDataString(zipCode)}";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("data", out var dataArray) && dataArray.GetArrayLength() > 0)
            {
                var firstResult = dataArray[0];
                double lat = firstResult.GetProperty("latitude").GetDouble();
                double lon = firstResult.GetProperty("longitude").GetDouble();
                return (lat, lon);
            }

            // Return null if no data found
            return null;
        }
    }
}