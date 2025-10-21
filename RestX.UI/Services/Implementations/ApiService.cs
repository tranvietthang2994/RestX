using RestX.UI.Services.Interfaces;
using System.Text;
using System.Text.Json;

namespace RestX.UI.Services.Implementations
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiService(HttpClient httpClient, ILogger<ApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            try
            {
                _logger.LogInformation("GET request to: {Endpoint}", endpoint);
                var response = await _httpClient.GetAsync(endpoint);
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug("GET response: {Json}", json);
                    return JsonSerializer.Deserialize<T>(json, _jsonOptions);
                }
                else
                {
                    _logger.LogWarning("GET request failed with status: {StatusCode}", response.StatusCode);
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Error content: {ErrorContent}", errorContent);
                }
                
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling GET {Endpoint}", endpoint);
                return default;
            }
        }

        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            try
            {
                _logger.LogInformation("POST request to: {Endpoint}", endpoint);
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(endpoint, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug("POST response: {Json}", responseJson);
                    return JsonSerializer.Deserialize<TResponse>(responseJson, _jsonOptions);
                }
                else
                {
                    _logger.LogWarning("POST request failed with status: {StatusCode}", response.StatusCode);
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Error content: {ErrorContent}", errorContent);
                }
                
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling POST {Endpoint}", endpoint);
                return default;
            }
        }

        public async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            try
            {
                _logger.LogInformation("PUT request to: {Endpoint}", endpoint);
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PutAsync(endpoint, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug("PUT response: {Json}", responseJson);
                    return JsonSerializer.Deserialize<TResponse>(responseJson, _jsonOptions);
                }
                else
                {
                    _logger.LogWarning("PUT request failed with status: {StatusCode}", response.StatusCode);
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Error content: {ErrorContent}", errorContent);
                }
                
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling PUT {Endpoint}", endpoint);
                return default;
            }
        }

        public async Task<bool> DeleteAsync(string endpoint)
        {
            try
            {
                _logger.LogInformation("DELETE request to: {Endpoint}", endpoint);
                var response = await _httpClient.DeleteAsync(endpoint);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogDebug("DELETE successful for: {Endpoint}", endpoint);
                    return true;
                }
                else
                {
                    _logger.LogWarning("DELETE request failed with status: {StatusCode}", response.StatusCode);
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Error content: {ErrorContent}", errorContent);
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling DELETE {Endpoint}", endpoint);
                return false;
            }
        }

        public void SetAuthToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            _logger.LogDebug("Authorization token set");
        }

        public void ClearAuthToken()
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
            _logger.LogDebug("Authorization token cleared");
        }

        public async Task<string?> GetStringAsync(string endpoint)
        {
            try
            {
                _logger.LogInformation("GET string request to: {Endpoint}", endpoint);
                var response = await _httpClient.GetAsync(endpoint);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    _logger.LogWarning("GET string request failed with status: {StatusCode}", response.StatusCode);
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling GET string {Endpoint}", endpoint);
                return null;
            }
        }

        public async Task<string?> PostStringAsync<TRequest>(string endpoint, TRequest data)
        {
            try
            {
                _logger.LogInformation("POST string request to: {Endpoint}", endpoint);
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(endpoint, content);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    _logger.LogWarning("POST string request failed with status: {StatusCode}", response.StatusCode);
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling POST string {Endpoint}", endpoint);
                return null;
            }
        }
    }
}
