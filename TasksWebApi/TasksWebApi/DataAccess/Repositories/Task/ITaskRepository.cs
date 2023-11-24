using TasksWebApi.DataAccess.Entities;

namespace TasksWebApi.DataAccess.Repositories;

public interface ITaskRepository
{
    Task<int> GetTotalRecordsAsync(int taskListId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TaskEntity>> GetAllAsync(int taskListId, int pageSize, int pageNumber, CancellationToken cancellationToken = default);
    Task<TaskEntity> GetAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(TaskEntity entity, CancellationToken cancellationToken = default);
    Task<TaskEntity> AttachAsync(int id, byte[] rowVersion);
    Task DeleteAsync(int id, byte[] rowVersion, CancellationToken cancellationToken = default);
    Task DeleteAllAsync(int taskListId, CancellationToken cancellationToken = default);
}