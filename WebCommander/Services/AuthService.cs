using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using Microsoft.JSInterop;
using WebCommander.Models;

namespace WebCommander.Services
{
    public class AuthService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly string _sessionId;
        private const string AUTH_STORAGE_KEY = "fractalc2_auth";
        private AuthConfig? _cachedAuth;

        public AuthService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
            _sessionId = Guid.NewGuid().ToString();
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            var auth = await GetAuthConfigAsync();
            return auth != null && 
                   !string.IsNullOrWhiteSpace(auth.ServerUrl) && 
                   !string.IsNullOrWhiteSpace(auth.Username) && 
                   !string.IsNullOrWhiteSpace(auth.ApiKey);
        }

        public async Task ValidateConnectionAsync(TeamServerClient client)
        {
            await client.ValidateAuthAsync();
        }

        public async Task<AuthConfig?> GetAuthConfigAsync()
        {
            if (_cachedAuth != null)
                return _cachedAuth;

            try
            {
                var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", AUTH_STORAGE_KEY);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    _cachedAuth = JsonSerializer.Deserialize<AuthConfig>(json);
                    return _cachedAuth;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading auth config: {ex.Message}");
            }

            return null;
        }

        public async Task SaveAuthConfigAsync(AuthConfig config)
        {
            try
            {
                var json = JsonSerializer.Serialize(config);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", AUTH_STORAGE_KEY, json);
                _cachedAuth = config;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving auth config: {ex.Message}");
                throw;
            }
        }

        public async Task ClearAuthConfigAsync()
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", AUTH_STORAGE_KEY);
                _cachedAuth = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing auth config: {ex.Message}");
            }
        }

        public async Task<string> GenerateTokenAsync()
        {
            var auth = await GetAuthConfigAsync();
            if (auth == null)
                throw new InvalidOperationException("No authentication configuration found");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(auth.ApiKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { 
                    new Claim("id", auth.Username), 
                    new Claim("session", _sessionId) 
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
