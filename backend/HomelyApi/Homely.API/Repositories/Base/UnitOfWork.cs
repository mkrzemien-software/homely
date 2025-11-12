using Microsoft.EntityFrameworkCore.Storage;
using Homely.API.Data;
using Homely.API.Repositories.Interfaces;
using Homely.API.Repositories.Implementations;

namespace Homely.API.Repositories.Base;

public class UnitOfWork : IUnitOfWork
{
    private readonly HomelyDbContext _context;
    private IDbContextTransaction? _transaction;

    private IPlanTypeRepository? _planTypes;
    private IHouseholdRepository? _households;
    private IHouseholdMemberRepository? _householdMembers;
    private ICategoryTypeRepository? _categoryTypes;
    private ICategoryRepository? _categories;
    private IItemRepository? _items;
    private ITaskRepository? _tasks;
    private ITaskHistoryRepository? _tasksHistory;
    private IPlanUsageRepository? _planUsages;

    public UnitOfWork(HomelyDbContext context)
    {
        _context = context;
    }

    public IPlanTypeRepository PlanTypes
    {
        get
        {
            _planTypes ??= new PlanTypeRepository(_context);
            return _planTypes;
        }
    }

    public IHouseholdRepository Households
    {
        get
        {
            _households ??= new HouseholdRepository(_context);
            return _households;
        }
    }

    public IHouseholdMemberRepository HouseholdMembers
    {
        get
        {
            _householdMembers ??= new HouseholdMemberRepository(_context);
            return _householdMembers;
        }
    }

    public ICategoryTypeRepository CategoryTypes
    {
        get
        {
            _categoryTypes ??= new CategoryTypeRepository(_context);
            return _categoryTypes;
        }
    }

    public ICategoryRepository Categories
    {
        get
        {
            _categories ??= new CategoryRepository(_context);
            return _categories;
        }
    }

    public IItemRepository Items
    {
        get
        {
            _items ??= new ItemRepository(_context);
            return _items;
        }
    }

    public ITaskRepository Tasks
    {
        get
        {
            _tasks ??= new TaskRepository(_context);
            return _tasks;
        }
    }

    public ITaskHistoryRepository TasksHistory
    {
        get
        {
            _tasksHistory ??= new TaskHistoryRepository(_context);
            return _tasksHistory;
        }
    }

    public IPlanUsageRepository PlanUsages
    {
        get
        {
            _planUsages ??= new PlanUsageRepository(_context);
            return _planUsages;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("Transakcja już istnieje. Najpierw zatwierdź lub wycofaj bieżącą transakcję.");
        }

        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("Brak aktywnej transakcji do zatwierdzenia.");
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await _transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("Brak aktywnej transakcji do wycofania.");
        }

        try
        {
            await _transaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
