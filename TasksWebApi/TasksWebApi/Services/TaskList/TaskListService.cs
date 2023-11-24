using Microsoft.EntityFrameworkCore;
using TasksWebApi.Constants;
using TasksWebApi.DataAccess;
using TasksWebApi.DataAccess.Entities;
using TasksWebApi.DataAccess.Repositories;
using TasksWebApi.Exceptions;
using TasksWebApi.Mappers;
using TasksWebApi.Models;

namespace TasksWebApi.Services;

public class TaskListService : BaseService, ITaskListService
{
    private readonly ITaskListRepository _repository;
    private readonly IHttpContextService _httpContextService;
    
    public TaskListService(IUnitOfWork unitOfWork, ITaskListRepository repository, IHttpContextService httpContextService) : base(unitOfWork)
    {
        _repository = repository;
        _httpContextService = httpContextService;
    }
    
    public async Task<PaginationResponse<ReadTaskListResponse>> GetAllAsync(PaginationRequest request, CancellationToken cancellationToken = default)
    {
        var userId = _httpContextService.GetContextUser().Id;
        IEnumerable<TaskListEntity> allEntities = await _repository.GetAllAsync(userId, request.PageSize, request.PageNumber, cancellationToken);
        int count = await _repository.GetTotalRecordsAsync(userId, cancellationToken);
        return new PaginationResponse<ReadTaskListResponse>(count, allEntities.Select(x => x.ToReadingTaskList()));
    }

    public async Task<ReadTaskListResponse> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        TaskListEntity entity = await _repository.GetAsync(id, cancellationToken);
        return entity.ToReadingTaskList();
    }

    public async Task<int> AddAsync(CreateTaskListRequest businessModel, CancellationToken cancellationToken = default)
    {
        await ValidateEntityToAddAsync(businessModel);
            
        var entity = businessModel.ToTaskListEntity(_httpContextService);
        await _repository.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    public async Task UpdateAsync(UpdateTaskListRequest businessModel, CancellationToken cancellationToken = default)
    {
        TaskListEntity entity = await _repository.GetAsync(businessModel.Id, cancellationToken);
        await ValidateEntityToUpdateAsync(entity, businessModel);

        TaskListEntity entityToUpdate = await _repository.AttachAsync(businessModel.Id, businessModel.RowVersion);
        entityToUpdate.ModifyFromUpdatingTaskList( businessModel);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(DeleteRequest deleteRequest, CancellationToken cancellationToken = default)
    {
        TaskListEntity taskList = await _repository.GetAsync(deleteRequest.Id, cancellationToken);
        await ValidateEntityToDeleteAsync(taskList);

        await _repository.DeleteAsync(deleteRequest.Id, deleteRequest.RowVersion, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
    
    private async Task ValidateEntityToAddAsync(CreateTaskListRequest businessModel)
    {
        var userId = _httpContextService.GetContextUser().Id;
        bool existsTaskListName = await _repository.ExistsOtherListWithSameNameAsync(userId, 0, businessModel.Name);
        if (existsTaskListName)
            throw new NotValidOperationException(ErrorCodes.TASK_LIST_NAME_EXISTS, $"The name {businessModel.Name} already exists in other list");
    }

    private async Task ValidateEntityToUpdateAsync(TaskListEntity entity, UpdateTaskListRequest businessModel)
    {
        if (entity == null)
            throw new NotValidOperationException(ErrorCodes.ITEM_NOT_EXISTS, $"The list to update does not exits");

        var userId = _httpContextService.GetContextUser().Id;
        bool existsTaskListName = await _repository.ExistsOtherListWithSameNameAsync(userId, businessModel.Id, businessModel.Name);
        if (existsTaskListName)
            throw new NotValidOperationException(ErrorCodes.TASK_LIST_NAME_EXISTS, $"The name {businessModel.Name} already exists in other list");
        
        if (!entity.UserId.Equals(_httpContextService.GetContextUser().Id, StringComparison.InvariantCultureIgnoreCase))
            throw new ForbidenActionException();
        
        if (entity.RowVersion.SequenceEqual(businessModel.RowVersion) != true)
            throw new DbUpdateConcurrencyException();
    }

    private async Task ValidateEntityToDeleteAsync(TaskListEntity entity)
    {
        if (entity == null)
            throw new NotValidOperationException(ErrorCodes.ITEM_NOT_EXISTS, $"The list to remove does not exits");
        
        bool containsAnyTask = await _repository.ContainsAnyTaskAsync(entity.Id);
        if (containsAnyTask)
            throw new NotValidOperationException(ErrorCodes.TASK_LIST_WITH_TASKS, $"The list to remove has tasks");

        var aux = _httpContextService.GetContextUser().Id;
        if (!entity.UserId.Equals(_httpContextService.GetContextUser().Id, StringComparison.InvariantCultureIgnoreCase))
            throw new ForbidenActionException();
    }
}