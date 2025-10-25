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
}
