using Homely.API.Models.DTOs.Households;

namespace Homely.API.Services
{
    /// <summary>
    /// Service interface for household operations
    /// </summary>
    public interface IHouseholdService
    {
        /// <summary>
        /// Get household by ID
        /// </summary>
        /// <param name="householdId">Household ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Household DTO or null if not found</returns>
        Task<HouseholdDto?> GetByIdAsync(Guid householdId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get households for a specific user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of household DTOs</returns>
        Task<IEnumerable<HouseholdDto>> GetUserHouseholdsAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if a user is a member of the household
        /// </summary>
        /// <param name="householdId">Household ID</param>
        /// <param name="userId">User ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if user is a member, false otherwise</returns>
        Task<bool> CanUserAccessHouseholdAsync(Guid householdId, Guid userId, CancellationToken cancellationToken = default);
    }
}
