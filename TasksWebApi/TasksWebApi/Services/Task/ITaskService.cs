using TasksWebApi.Models;

namespace TasksWebApi.Services;

public interface ITaskService
{
    Task<PaginationResponse<ReadTaskResponse>> GetAllAsync(TaskPaginationRequest request, CancellationToken cancellationToken = default);
    Task<ReadTaskResponse> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<int> AddAsync(CreateTaskRequest businessModel, CancellationToken cancellationToken = default);
    Task UpdateAsync(UpdateTaskRequest businessModel, CancellationToken cancellationToken = default);
    Task DeleteAsync(DeleteRequest deleteRequest, CancellationToken cancellationToken = default);
    Task DeleteAllInListAsync(DeleteRequest deleteRequest, CancellationToken cancellationToken = default);
}