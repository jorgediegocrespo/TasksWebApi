namespace TasksWebApi.DataAccess;

public class UnitOfWork(TasksDbContext tasksDbContext) : IUnitOfWork
{
    public void ClearTracker()
    {
        tasksDbContext.ChangeTracker.Clear();
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return tasksDbContext.SaveChangesAsync(cancellationToken);
    }
    
    public Task<int> SaveChangesWithoutSoftDeleteAsync(CancellationToken cancellationToken = default)
    {
        return tasksDbContext.SaveChangesWithoutSoftDeleteAsync(cancellationToken);
    }

    public async Task SaveChangesInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        await using var transaction = await tasksDbContext.Database.BeginTransactionAsync(cancellationToken);
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