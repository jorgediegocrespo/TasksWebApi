namespace TasksWebApi.DataAccess;

public interface IUnitOfWork
{
    void ClearTracker();
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    public Task<int> SaveChangesInTransactionAsync(Func<Task<int>> operation, CancellationToken cancellationToken = default);
    Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default);
}