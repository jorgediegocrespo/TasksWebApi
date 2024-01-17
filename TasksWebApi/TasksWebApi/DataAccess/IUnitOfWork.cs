namespace TasksWebApi.DataAccess;

public interface IUnitOfWork
{
    void ClearTracker();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<int> SaveChangesWithoutSoftDeleteAsync(CancellationToken cancellationToken = default);
    Task SaveChangesInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default);
}