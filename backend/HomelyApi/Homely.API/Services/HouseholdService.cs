using Homely.API.Models.DTOs.Households;
using Homely.API.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Homely.API.Services
{
    /// <summary>
    /// Service for household operations
    /// </summary>
    public class HouseholdService : IHouseholdService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<HouseholdService> _logger;

        public HouseholdService(
            IUnitOfWork unitOfWork,
            ILogger<HouseholdService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<HouseholdDto?> GetByIdAsync(Guid householdId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Fetching household with ID: {HouseholdId}", householdId);

                var household = await _unitOfWork.Households.GetHouseholdWithMembersAsync(householdId, cancellationToken);

                if (household == null)
                {
                    _logger.LogWarning("Household not found: {HouseholdId}", householdId);
                    return null;
                }

                // Map to DTO
                var dto = new HouseholdDto
                {
                    Id = household.Id.ToString(),
                    Name = household.Name,
                    Address = household.Address,
                    PlanTypeId = household.PlanTypeId,
                    PlanTypeName = household.PlanType?.Name ?? "Unknown",
                    SubscriptionStatus = household.SubscriptionStatus,
                    SubscriptionStartDate = household.SubscriptionStartDate,
                    SubscriptionEndDate = household.SubscriptionEndDate,
                    MemberCount = household.HouseholdMembers?.Count(m => m.DeletedAt == null) ?? 0,
                    CreatedAt = household.CreatedAt
                };

                _logger.LogInformation("Successfully fetched household: {HouseholdName}", household.Name);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching household: {HouseholdId}", householdId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<HouseholdDto>> GetUserHouseholdsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Fetching households for user: {UserId}", userId);

                var households = await _unitOfWork.Households.GetUserHouseholdsAsync(userId, cancellationToken);

                var dtos = households.Select(h => new HouseholdDto
                {
                    Id = h.Id.ToString(),
                    Name = h.Name,
                    Address = h.Address,
                    PlanTypeId = h.PlanTypeId,
                    PlanTypeName = h.PlanType?.Name ?? "Unknown",
                    SubscriptionStatus = h.SubscriptionStatus,
                    SubscriptionStartDate = h.SubscriptionStartDate,
                    SubscriptionEndDate = h.SubscriptionEndDate,
                    MemberCount = h.HouseholdMembers?.Count(m => m.DeletedAt == null) ?? 0,
                    CreatedAt = h.CreatedAt
                }).ToList();

                _logger.LogInformation("Found {Count} households for user: {UserId}", dtos.Count, userId);
                return dtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching households for user: {UserId}", userId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> CanUserAccessHouseholdAsync(Guid householdId, Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _unitOfWork.Households.IsUserMemberAsync(householdId, userId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking user access for household: {HouseholdId}, User: {UserId}", householdId, userId);
                throw;
            }
        }
    }
}
