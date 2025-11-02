using Homely.API.Configuration;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Homely.API.Services;

/// <summary>
/// Service implementation for Supabase Auth admin operations
/// Uses Supabase Admin API with service role key
/// </summary>
public class SupabaseAuthService : ISupabaseAuthService
{
    private readonly HttpClient _httpClient;
    private readonly SupabaseSettings _supabaseSettings;
    private readonly ILogger<SupabaseAuthService> _logger;

    public SupabaseAuthService(
        IHttpClientFactory httpClientFactory,
        IOptions<SupabaseSettings> supabaseSettings,
        ILogger<SupabaseAuthService> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _supabaseSettings = supabaseSettings.Value;
        _logger = logger;

        // Configure HttpClient with Supabase base URL and authorization
        _httpClient.BaseAddress = new Uri(_supabaseSettings.Url);
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _supabaseSettings.ServiceRoleKey);
        _httpClient.DefaultRequestHeaders.Add("apikey", _supabaseSettings.ServiceRoleKey);
    }

    /// <summary>
    /// Create a new user in Supabase Auth
    /// </summary>
    public async Task<Guid> CreateUserAsync(
        string email,
        string? password = null,
        bool emailConfirm = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating user in Supabase Auth: {Email}", email);

            // Generate random password if not provided
            var userPassword = password ?? GenerateRandomPassword();

            var requestBody = new
            {
                email,
                password = userPassword,
                email_confirm = emailConfirm,
                user_metadata = new { }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/auth/v1/admin/users", content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to create user in Supabase Auth. Status: {StatusCode}, Error: {Error}",
                    response.StatusCode, errorContent);
                throw new InvalidOperationException($"Failed to create user in Supabase Auth: {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogDebug("Supabase Auth response: {Response}", responseContent);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var userResponse = JsonSerializer.Deserialize<SupabaseUserResponse>(responseContent, options);

            if (userResponse?.Id == null)
            {
                _logger.LogError("User ID not returned from Supabase Auth. Response: {Response}", responseContent);
                throw new InvalidOperationException("User ID not returned from Supabase Auth");
            }

            _logger.LogInformation("User created successfully in Supabase Auth: {UserId}", userResponse.Id);
            return Guid.Parse(userResponse.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user in Supabase Auth: {Email}", email);
            throw;
        }
    }

    /// <summary>
    /// Delete a user from Supabase Auth
    /// </summary>
    public async Task<bool> DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting user from Supabase Auth: {UserId}", userId);

            var response = await _httpClient.DeleteAsync(
                $"/auth/v1/admin/users/{userId}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to delete user from Supabase Auth. Status: {StatusCode}, Error: {Error}",
                    response.StatusCode, errorContent);

                // If user not found, consider it as already deleted
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("User {UserId} not found in Supabase Auth, considering as deleted", userId);
                    return true;
                }

                return false;
            }

            _logger.LogInformation("User deleted successfully from Supabase Auth: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user from Supabase Auth: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Generate a random secure password
    /// </summary>
    private static string GenerateRandomPassword()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 16)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    /// <summary>
    /// Supabase user response model
    /// </summary>
    private class SupabaseUserResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("aud")]
        public string? Aud { get; set; }

        [JsonPropertyName("role")]
        public string? Role { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("email_confirmed_at")]
        public string? EmailConfirmedAt { get; set; }

        [JsonPropertyName("phone")]
        public string? Phone { get; set; }

        [JsonPropertyName("created_at")]
        public string? CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public string? UpdatedAt { get; set; }
    }
}
