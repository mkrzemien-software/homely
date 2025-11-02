namespace Homely.API.Services;

/// <summary>
/// Service interface for Supabase Auth admin operations
/// </summary>
public interface ISupabaseAuthService
{
    /// <summary>
    /// Create a new user in Supabase Auth
    /// </summary>
    /// <param name="email">User email</param>
    /// <param name="password">User password (optional, will be auto-generated if not provided)</param>
    /// <param name="emailConfirm">Auto-confirm email (default: true for admin creation)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created user ID</returns>
    Task<Guid> CreateUserAsync(
        string email,
        string? password = null,
        bool emailConfirm = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a user from Supabase Auth
    /// </summary>
    /// <param name="userId">User ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
