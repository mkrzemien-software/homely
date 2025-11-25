using Homely.API.Entities;
using Homely.API.Repositories.Base;

namespace Homely.API.Repositories.Interfaces;

public interface IHouseholdRepository : IBaseRepository<HouseholdEntity, Guid>
{
    Task<IEnumerable<HouseholdEntity>> GetUserHouseholdsAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<HouseholdEntity?> GetHouseholdWithMembersAsync(Guid householdId, CancellationToken cancellationToken = default);

    Task<HouseholdEntity?> GetHouseholdWithDetailsAsync(Guid householdId, CancellationToken cancellationToken = default);

    Task<bool> IsUserMemberAsync(Guid householdId, Guid userId, CancellationToken cancellationToken = default);

    Task<bool> IsUserAdminAsync(Guid householdId, Guid userId, CancellationToken cancellationToken = default);

    // System Developer methods
    Task<PagedResult<HouseholdEntity>> SearchHouseholdsAsync(
        string? searchTerm,
        int? planTypeId,
        string? subscriptionStatus,
        bool? hasActiveMembers,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<HouseholdStatsResult> GetHouseholdStatsAsync(CancellationToken cancellationToken = default);
}

public class HouseholdStatsResult
{
    public int TotalHouseholds { get; set; }
    public int ActiveHouseholds { get; set; }
    public int FreeHouseholds { get; set; }
    public int PremiumHouseholds { get; set; }
    public int TotalMembers { get; set; }
    public int TotalTasks { get; set; }
    public int TotalEvents { get; set; }
}
