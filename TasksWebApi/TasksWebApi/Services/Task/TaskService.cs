using Microsoft.EntityFrameworkCore;
using TasksWebApi.Constants;
using TasksWebApi.DataAccess;
using TasksWebApi.DataAccess.Entities;
using TasksWebApi.DataAccess.Repositories;
using TasksWebApi.Exceptions;
using TasksWebApi.Mappers;
using TasksWebApi.Models;

namespace TasksWebApi.Services;

public class TaskService : BaseService, ITaskService
{
    private readonly ITaskRepository _repository;
    private readonly ITaskListRepository _taskListRepository;
    private readonly IHttpContextService _httpContextService;
    
    public TaskService(IUnitOfWork unitOfWork, ITaskRepository repository, ITaskListRepository taskListRepository, IHttpContextService httpContextService) : base(unitOfWork)
    {
        _repository = repository;
        _taskListRepository = taskListRepository;
        _httpContextService = httpContextService;
    }

    public async Task<PaginationResponse<ReadTaskResponse>> GetAllAsync(TaskPaginationRequest request, CancellationToken cancellationToken = default)
    {
        IEnumerable<TaskEntity> allEntities = await _repository.GetAllAsync(request.TaskListId, request.PageSize, request.PageNumber, cancellationToken);
        int count = await _repository.GetTotalRecordsAsync(request.TaskListId, cancellationToken);
        return new PaginationResponse<ReadTaskResponse>(count, allEntities.Select(x => x.ToReadingTask()));
    }

    public async Task<ReadTaskResponse> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        TaskEntity entity = await _repository.GetAsync(id, cancellationToken);
        return entity.ToReadingTask();
    }

    public async Task<int> AddAsync(CreateTaskRequest businessModel, CancellationToken cancellationToken = default)
    {
        await ValidateEntityToAddAsync(businessModel);
            
        var entity = businessModel.ToTaskEntity();
        await _repository.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    public async Task UpdateAsync(UpdateTaskRequest businessModel, CancellationToken cancellationToken = default)
    {
        TaskEntity entity = await _repository.GetAsync(businessModel.Id, cancellationToken);
        await ValidateEntityToUpdateAsync(entity, businessModel);

        TaskEntity entityToUpdate = await _repository.AttachAsync(businessModel.Id, businessModel.RowVersion);
        entityToUpdate.ModifyFromUpdatingTask(businessModel);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(DeleteRequest deleteRequest, CancellationToken cancellationToken = default)
    {
        TaskEntity entity = await _repository.GetAsync(deleteRequest.Id, cancellationToken);
        ValidateEntityToDelete(entity);

        await _repository.DeleteAsync(deleteRequest.Id, deleteRequest.RowVersion, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAllInListAsync(DeleteRequest deleteRequest, CancellationToken cancellationToken = default)
    {
        var entity = await _taskListRepository.GetAsync(deleteRequest.Id, cancellationToken);
        ValidateEntityToDeleteAllInList(entity, deleteRequest);

        await _repository.DeleteAllAsync(deleteRequest.Id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
    
    private async Task ValidateEntityToAddAsync(CreateTaskRequest businessModel)
    {
        var userId = _httpContextService.GetContextUser().Id;
        bool existsList = await _taskListRepository.ExistsAsync(userId, businessModel.TaskListId);
        if (!existsList) 
            throw new NotValidOperationException(ErrorCodes.TASK_LIST_NOT_EXISTS, $"The list to add the task into does not exits");
    }

    private async Task ValidateEntityToUpdateAsync(TaskEntity entity, UpdateTaskRequest businessModel)
    {
        if (entity == null)
            throw new NotValidOperationException(ErrorCodes.ITEM_NOT_EXISTS, $"The task to update does not exits");
        
        var userId = _httpContextService.GetContextUser().Id;
        bool existsList = await _taskListRepository.ExistsAsync(userId, businessModel.TaskListId);
        if (!existsList) 
            throw new NotValidOperationException(ErrorCodes.TASK_LIST_NOT_EXISTS, $"The list to add the task into does not exits");
        
        if (entity.RowVersion.SequenceEqual(businessModel.RowVersion) != true)
            throw new DbUpdateConcurrencyException();
    }

    private void ValidateEntityToDelete(TaskEntity entity)
    {
        if (entity == null)
            throw new NotValidOperationException(ErrorCodes.ITEM_NOT_EXISTS, $"The task to remove does not exits");
    }
    
    private void ValidateEntityToDeleteAllInList(TaskListEntity entity, DeleteRequest businessModel)
    {
        if (entity == null)
            throw new NotValidOperationException(ErrorCodes.ITEM_NOT_EXISTS, $"The task list to remove all task in does not exits");
        
        if (entity.RowVersion.SequenceEqual(businessModel.RowVersion) != true)
            throw new DbUpdateConcurrencyException();
    }
}