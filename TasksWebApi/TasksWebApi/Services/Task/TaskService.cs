using Microsoft.EntityFrameworkCore;
using TasksWebApi.Constants;
using TasksWebApi.DataAccess;
using TasksWebApi.DataAccess.Entities;
using TasksWebApi.DataAccess.Repositories;
using TasksWebApi.Exceptions;
using TasksWebApi.Mappers;
using TasksWebApi.Models;

namespace TasksWebApi.Services;

public class TaskService(
    IUnitOfWork unitOfWork,
    ITaskRepository repository,
    ITaskListRepository taskListRepository,
    IHttpContextService httpContextService,
    ILogger<TaskService> loggerManager)
    : BaseService(unitOfWork), ITaskService
{
    public async Task<PaginationResponse<ReadTaskResponse>> GetAllAsync(TaskPaginationRequest request, CancellationToken cancellationToken = default)
    {
        var allEntities = await repository.GetAllAsync(request.TaskListId, request.PageSize, request.PageNumber, cancellationToken);
        var count = await repository.GetTotalRecordsAsync(request.TaskListId, cancellationToken);
        return new PaginationResponse<ReadTaskResponse>(count, allEntities.Select(x => x.ToReadingTask()));
    }

    public async Task<ReadTaskResponse> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetAsync(id, cancellationToken);
        return entity.ToReadingTask();
    }

    public async Task<int> AddAsync(CreateTaskRequest businessModel, CancellationToken cancellationToken = default)
    {
        await ValidateEntityToAddAsync(businessModel);
            
        var entity = businessModel.ToTaskEntity();
        await repository.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    public async Task UpdateAsync(UpdateTaskRequest businessModel, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetAsync(businessModel.Id, cancellationToken);
        await ValidateEntityToUpdateAsync(entity, businessModel);

        var entityToUpdate = await repository.AttachAsync(businessModel.Id, businessModel.RowVersion);
        entityToUpdate.ModifyFromUpdatingTask(businessModel);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(DeleteRequest deleteRequest, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetAsync(deleteRequest.Id, cancellationToken);
        ValidateEntityToDelete(entity);

        await repository.DeleteAsync(deleteRequest.Id, deleteRequest.RowVersion, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAllInListAsync(DeleteRequest deleteRequest, CancellationToken cancellationToken = default)
    {
        var entity = await taskListRepository.GetAsync(deleteRequest.Id, cancellationToken);
        ValidateEntityToDeleteAllInList(entity, deleteRequest);

        await repository.DeleteAllAsync(deleteRequest.Id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
    
    private async Task ValidateEntityToAddAsync(CreateTaskRequest businessModel)
    {
        var userId = httpContextService.GetContextUser().Id;
        var existsList = await taskListRepository.ExistsAsync(userId, businessModel.TaskListId);
        if (!existsList)
        {
            loggerManager.LogInformation("The list to add the task into does not exits");
            throw new NotValidOperationException(ErrorCodes.TASK_LIST_NOT_EXISTS, "The list to add the task into does not exits");
        }
    }

    private async Task ValidateEntityToUpdateAsync(TaskEntity entity, UpdateTaskRequest businessModel)
    {
        if (entity == null)
        {
            loggerManager.LogInformation("The task to update does not exits");
            throw new NotValidOperationException(ErrorCodes.ITEM_NOT_EXISTS, "The task to update does not exits");
        }
        
        var userId = httpContextService.GetContextUser().Id;
        var existsList = await taskListRepository.ExistsAsync(userId, businessModel.TaskListId);
        if (!existsList)
        {
            loggerManager.LogInformation("The list to add the task into does not exits");
            throw new NotValidOperationException(ErrorCodes.TASK_LIST_NOT_EXISTS, "The list to add the task into does not exits");
        }
        
        if (entity.RowVersion.SequenceEqual(businessModel.RowVersion) != true)
        {
            loggerManager.LogInformation("The list to update has been modified by other user");
            throw new DbUpdateConcurrencyException();
        }
    }

    private void ValidateEntityToDelete(TaskEntity entity)
    {
        if (entity == null)
        {
            loggerManager.LogInformation("The task to remove does not exits");
            throw new NotValidOperationException(ErrorCodes.ITEM_NOT_EXISTS, "The task to remove does not exits");
        }
    }
    
    private void ValidateEntityToDeleteAllInList(TaskListEntity entity, DeleteRequest businessModel)
    {
        if (entity == null)
        {
            loggerManager.LogInformation("The task list to remove all task in does not exits");
            throw new NotValidOperationException(ErrorCodes.ITEM_NOT_EXISTS, "The task list to remove all task in does not exits");
        }

        if (entity.RowVersion.SequenceEqual(businessModel.RowVersion) != true)
        {
            loggerManager.LogInformation("The list to update has been modified by other user");
            throw new DbUpdateConcurrencyException();
        }
    }
}