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
    /// Create new household with admin assignment
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
    /// Assign admin to household
    /// </summary>
    Task<SystemHouseholdDto> AssignAdminAsync(
        AssignAdminDto assignDto,
        CancellationToken cancellationToken = default);
}
