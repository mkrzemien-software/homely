using Homely.API.Models.DTOs.SystemHouseholds;

namespace Homely.API.Services;

/// <summary>
/// Service interface for system-level household management
/// </summary>
public interface ISystemHouseholdsService
{
    /// <summary>
    /// Search households with filtering and pagination
    /// </summary>
    Task<PaginatedHouseholdsDto> SearchHouseholdsAsync(
        HouseholdSearchFiltersDto filters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get detailed household information
    /// </summary>
    Task<SystemHouseholdDetailsDto?> GetHouseholdDetailsAsync(
        Guid householdId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get overall system household statistics
    /// </summary>
    Task<HouseholdStatsDto> GetHouseholdStatsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create new household with optional admin assignment.
    /// If AdminUserId is provided, the user will be added as household admin.
    /// If AdminUserId is not provided, admin can be assigned later via AssignAdminAsync.
    /// </summary>
    Task<SystemHouseholdDto> CreateHouseholdAsync(
        CreateHouseholdDto createDto,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update household information
    /// </summary>
    Task<SystemHouseholdDto> UpdateHouseholdAsync(
        Guid householdId,
        UpdateHouseholdDto updateDto,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete household (soft delete)
    /// </summary>
    Task<bool> DeleteHouseholdAsync(
        Guid householdId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Assign admin to household. If the user is already a member, their role will be updated to admin.
    /// If the user is not a member, they will be added as an admin member.
    /// </summary>
    Task<SystemHouseholdDto> AssignAdminAsync(
        AssignAdminDto assignDto,
        CancellationToken cancellationToken = default);
}
