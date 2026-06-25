using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.DTOs.Nomba;
using AjoCoreBackend.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;

namespace AjoCoreBackend.Infrastructure.Services
{
    public class NombaTokenService : INombaTokenService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        
        private string? _cachedToken;
        private DateTime _tokenExpiry = DateTime.MinValue;

        public NombaTokenService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiry)
            {
                return _cachedToken;
            }

            await _semaphore.WaitAsync();
            try
            {
                // Double-check inside the lock
                if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiry)
                {
                    return _cachedToken;
                }

                var clientId = _configuration["Nomba:ClientId"];
                var clientSecret = _configuration["Nomba:ClientSecret"];

                var requestPayload = new
                {
                    grant_type = "client_credentials",
                    client_id = clientId,
                    client_secret = clientSecret
                };

                var response = await _httpClient.PostAsJsonAsync("/v1/auth/token/issue", requestPayload);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                
                // Using JsonElement as Nomba wraps responses differently sometimes, adjust based on actual Nomba response structure
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;
                
                // Assuming standard OAuth2 response structure from Nomba
                if (root.TryGetProperty("access_token", out var accessTokenProp))
                {
                    _cachedToken = accessTokenProp.GetString();
                    
                    // Spec mandates caching for 55 minutes to be safe
                    _tokenExpiry = DateTime.UtcNow.AddMinutes(55);
                    
                    return _cachedToken ?? string.Empty;
                }
                
                throw new Exception("Failed to extract access_token from Nomba response.");
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
