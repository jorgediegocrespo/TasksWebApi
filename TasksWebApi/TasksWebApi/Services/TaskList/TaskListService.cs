using Microsoft.EntityFrameworkCore;
using TasksWebApi.Constants;
using TasksWebApi.DataAccess;
using TasksWebApi.DataAccess.Entities;
using TasksWebApi.DataAccess.Repositories;
using TasksWebApi.Exceptions;
using TasksWebApi.Mappers;
using TasksWebApi.Models;

namespace TasksWebApi.Services;

public class TaskListService(
    IUnitOfWork unitOfWork,
    ITaskListRepository repository,
    ICacheService cacheService,
    IHttpContextService httpContextService,
    ILogger<TaskListService> loggerManager)
    : BaseService(unitOfWork), ITaskListService
{
    public async Task<PaginationResponse<ReadTaskListResponse>> GetAllAsync(PaginationRequest request, CancellationToken cancellationToken = default)
    {
        var userId = httpContextService.GetContextUser().Id;
        var cacheKey = $"{userId}_TaskList_{request.PageNumber}_{request.PageSize}";
        var cachedData = await cacheService.GetAsync<PaginationResponse<ReadTaskListResponse>>(cacheKey, null, cancellationToken);
        if (cachedData != null)
            return cachedData;
        
        var allEntities = await repository.GetAllAsync(userId, request.PageSize, request.PageNumber, cancellationToken);
        var count = await repository.GetTotalRecordsAsync(userId, cancellationToken);
        var result = new PaginationResponse<ReadTaskListResponse>(count, allEntities.Select(x => x.ToReadingTaskList()));
        await cacheService.SetWithDefaultExpirationAsync(cacheKey, result, cancellationToken);

        return result;
    }

    public async Task<ReadTaskListResponse> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetAsync(id, cancellationToken);
        return entity.ToReadingTaskList();
    }

    public async Task<int> AddAsync(CreateTaskListRequest businessModel, CancellationToken cancellationToken = default)
    {
        await ValidateEntityToAddAsync(businessModel);
            
        var entity = businessModel.ToTaskListEntity(httpContextService);
        await repository.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    public async Task UpdateAsync(UpdateTaskListRequest businessModel, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetAsync(businessModel.Id, cancellationToken);
        await ValidateEntityToUpdateAsync(entity, businessModel);

        var entityToUpdate = await repository.AttachAsync(businessModel.Id, businessModel.RowVersion);
        entityToUpdate.ModifyFromUpdatingTaskList( businessModel);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(DeleteRequest deleteRequest, CancellationToken cancellationToken = default)
    {
        var taskList = await repository.GetAsync(deleteRequest.Id, cancellationToken);
        await ValidateEntityToDeleteAsync(taskList);

        await repository.DeleteAsync(deleteRequest.Id, deleteRequest.RowVersion, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
    
    private async Task ValidateEntityToAddAsync(CreateTaskListRequest businessModel)
    {
        var userId = httpContextService.GetContextUser().Id;
        var existsTaskListName = await repository.ExistsOtherListWithSameNameAsync(userId, 0, businessModel.Name);
        if (existsTaskListName)
        {
            loggerManager.LogInformation($"The name {businessModel.Name} already exists in other list");
            throw new NotValidOperationException(ErrorCodes.TASK_LIST_NAME_EXISTS, $"The name {businessModel.Name} already exists in other list");
        }
    }

    private async Task ValidateEntityToUpdateAsync(TaskListEntity entity, UpdateTaskListRequest businessModel)
    {
        if (entity == null)
        {
            loggerManager.LogInformation("The list to update does not exits");
            throw new NotValidOperationException(ErrorCodes.ITEM_NOT_EXISTS, "The list to update does not exits");
        }

        var userId = httpContextService.GetContextUser().Id;
        var existsTaskListName = await repository.ExistsOtherListWithSameNameAsync(userId, businessModel.Id, businessModel.Name);
        if (existsTaskListName)
        {
            loggerManager.LogInformation($"The name {businessModel.Name} already exists in other list");
            throw new NotValidOperationException(ErrorCodes.TASK_LIST_NAME_EXISTS, $"The name {businessModel.Name} already exists in other list");
        }

        if (!entity.UserId.Equals(httpContextService.GetContextUser().Id, StringComparison.InvariantCultureIgnoreCase))
        {
            loggerManager.LogInformation("The list to update does not belongs to the user");
            throw new ForbidenActionException();
        }

        if (entity.RowVersion.SequenceEqual(businessModel.RowVersion) != true)
        {
            loggerManager.LogInformation("The list to update has been modified by other user");
            throw new DbUpdateConcurrencyException();
        }
    }

    private async Task ValidateEntityToDeleteAsync(TaskListEntity entity)
    {
        if (entity == null)
        {
            loggerManager.LogInformation("The list to remove does not exits");
            throw new NotValidOperationException(ErrorCodes.ITEM_NOT_EXISTS, "The list to remove does not exits");
        }

        var containsAnyTask = await repository.ContainsAnyTaskAsync(entity.Id);
        if (containsAnyTask)
        {
            loggerManager.LogInformation("The list to remove has tasks");
            throw new NotValidOperationException(ErrorCodes.TASK_LIST_WITH_TASKS, "The list to remove has tasks");
        }

        if (!entity.UserId.Equals(httpContextService.GetContextUser().Id, StringComparison.InvariantCultureIgnoreCase))
        {
            loggerManager.LogInformation("The list to remove does not belongs to the user");
            throw new ForbidenActionException();
        }
    }
}