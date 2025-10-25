using Homely.API.Entities;
using Homely.API.Repositories.Base;

namespace Homely.API.Repositories.Interfaces;

public interface IHouseholdMemberRepository : IBaseRepository<HouseholdMemberEntity, Guid>
{
    Task<IEnumerable<HouseholdMemberEntity>> GetHouseholdMembersAsync(Guid householdId, CancellationToken cancellationToken = default);

    Task<IEnumerable<HouseholdMemberEntity>> GetUserMembershipsAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<HouseholdMemberEntity?> GetMembershipAsync(Guid householdId, Guid userId, CancellationToken cancellationToken = default);

    Task<HouseholdMemberEntity?> GetByInvitationTokenAsync(string invitationToken, CancellationToken cancellationToken = default);

    Task<bool> HasRoleAsync(Guid householdId, Guid userId, string role, CancellationToken cancellationToken = default);

    Task<IEnumerable<HouseholdMemberEntity>> GetAdminsAsync(Guid householdId, CancellationToken cancellationToken = default);
}
