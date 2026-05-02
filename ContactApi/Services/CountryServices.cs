using ContactApi.Dto;
using System.Net.Http.Json;
using System.Text.Json;

namespace ContactApi.Services
{
    /// <summary>
    /// Service for interacting with external Country data.
    /// Built to support scalable and data-driven backend operations.
    /// </summary>
    public class CountryServices : ICountryServices
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CountryServices> _logger;

        // Optimized for case-insensitive JSON coming from external REST APIs
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        // Standardized field filtering to minimize payload size and improve latency
        private const string FilteredFields = "fields=name,region,subregion,population";

        public CountryServices(HttpClient httpClient, ILogger<CountryServices> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves all countries with optimized field selection.
        /// </summary>
        public async Task<IEnumerable<CountryDto>> GetAllCountriesAsync()
        {
            const string endpoint = $"all?{FilteredFields}";

            try
            {
                _logger.LogInformation("Initiating request to: {Uri}", _httpClient.BaseAddress + endpoint);

                var response = await _httpClient.GetAsync(endpoint);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("External API failure: {StatusCode} at {Uri}", response.StatusCode, endpoint);
                    return Enumerable.Empty<CountryDto>();
                }

                var countries = await response.Content.ReadFromJsonAsync<List<CountryApiResponse>>(_jsonOptions);

                if (countries == null || !countries.Any())
                {
                    _logger.LogWarning("API returned success, but country list was empty.");
                    return Enumerable.Empty<CountryDto>();
                }

                _logger.LogInformation("Successfully processed {Count} countries.", countries.Count);
                return countries.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "System failure during GetAllCountriesAsync.");
                return Enumerable.Empty<CountryDto>();
            }
        }

        /// <summary>
        /// Retrieves a specific country by name.
        /// </summary>
        public async Task<CountryDto?> GetCountryByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;

            string endpoint = $"name/{name}?{FilteredFields}";

            try
            {
                _logger.LogInformation("Searching for country: {CountryName}", name);

                var countries = await _httpClient.GetFromJsonAsync<List<CountryApiResponse>>(endpoint, _jsonOptions);

                var result = countries?.FirstOrDefault();
                if (result == null)
                {
                    _logger.LogWarning("No match found for country: {CountryName}", name);
                    return null;
                }

                return MapToDto(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while searching for {CountryName}", name);
                return null;
            }
        }

        /// <summary>
        /// Maps internal API models to professional DTOs, ensuring data integrity.
        /// </summary>
        private static CountryDto MapToDto(CountryApiResponse apiModel)
        {
            return new CountryDto
            {
                Name = new Name
                {
                    common = apiModel.Name?.Common ?? "N/A",
                    official = apiModel.Name?.Official ?? "N/A"
                },
                Region = apiModel.Region ?? "N/A",
                Subregion = apiModel.Subregion ?? "N/A",
                Population = apiModel.Population
            };
        }

        #region Internal API Models
        // These models are kept private to the service to prevent leaking 
        // external dependencies into the rest of the application.

        private class CountryApiResponse
        {
            public NameResponse Name { get; set; } = new();
            public string? Region { get; set; }
            public string? Subregion { get; set; }
            public long Population { get; set; }
        }

        private class NameResponse
        {
            public string? Common { get; set; }
            public string? Official { get; set; }
        }
        #endregion
    }
}