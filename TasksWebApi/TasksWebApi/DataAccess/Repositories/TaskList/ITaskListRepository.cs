using TasksWebApi.DataAccess.Entities;

namespace TasksWebApi.DataAccess.Repositories;

public interface ITaskListRepository
{
    Task<bool> ExistsAsync(string userId, int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsOtherListWithSameNameAsync(string userId, int id, string name, CancellationToken cancellationToken = default);
    Task<int> GetTotalRecordsAsync(string userId, CancellationToken cancellationToken = default, bool includeDeleted = false);
    Task<IEnumerable<TaskListEntity>> GetAllAsync(string userId, int pageSize, int pageNumber, CancellationToken cancellationToken = default);
    Task<TaskListEntity> GetAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(TaskListEntity entity, CancellationToken cancellationToken = default);
    Task<TaskListEntity> AttachAsync(int id, byte[] rowVersion);
    Task DeleteAsync(int id, byte[] rowVersion, CancellationToken cancellationToken = default);
    Task<bool> ContainsAnyTaskAsync(int id, CancellationToken cancellationToken = default);
}