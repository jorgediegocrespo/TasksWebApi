using TasksWebApi.Models;

namespace TasksWebApi.Services;

public interface ITaskListService : IBaseService
{
    Task<PaginationResponse<ReadTaskListResponse>> GetAllAsync(PaginationRequest request, CancellationToken cancellationToken = default);
    Task<ReadTaskListResponse> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<int> AddAsync(CreateTaskListRequest businessModel, CancellationToken cancellationToken = default);
    Task UpdateAsync(UpdateTaskListRequest businessModel, CancellationToken cancellationToken = default);
    Task DeleteAsync(DeleteRequest deleteRequest, CancellationToken cancellationToken = default);
}