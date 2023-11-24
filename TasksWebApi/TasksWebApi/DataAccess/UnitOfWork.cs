namespace TasksWebApi.DataAccess;

public class UnitOfWork : IUnitOfWork
{
    private readonly TasksDbContext _tasksDbContext;

    public UnitOfWork(TasksDbContext tasksDbContext)
    {
        _tasksDbContext = tasksDbContext;
    }

    public void ClearTracker()
    {
        _tasksDbContext.ChangeTracker.Clear();
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _tasksDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> SaveChangesInTransactionAsync(Func<Task<int>> operation, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _tasksDbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            int result = await operation();
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _tasksDbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await operation();
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}