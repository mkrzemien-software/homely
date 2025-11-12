using Homely.API.Repositories.Interfaces;

namespace Homely.API.Repositories.Base;

public interface IUnitOfWork : IDisposable
{
    IPlanTypeRepository PlanTypes { get; }
    IHouseholdRepository Households { get; }
    IHouseholdMemberRepository HouseholdMembers { get; }
    ICategoryTypeRepository CategoryTypes { get; }
    ICategoryRepository Categories { get; }
    IItemRepository Items { get; }
    ITaskRepository Tasks { get; }
    ITaskHistoryRepository TasksHistory { get; }
    IPlanUsageRepository PlanUsages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
