using TasksWebApi.DataAccess.Entities;
using TasksWebApi.Models;
using TasksWebApi.Services;

namespace TasksWebApi.Mappers;

public static class TaskListMappers
{
    public static ReadTaskListResponse ToReadingTaskList(this TaskListEntity entity)
    {
        return entity == null ? 
            null : 
            new ReadTaskListResponse(entity.Id, entity.RowVersion, entity.Name);
    }

    public static TaskListEntity ToTaskListEntity(this CreateTaskListRequest businessModel, IHttpContextService httpContextService)
    {
        return new TaskListEntity()
        {
            Name = businessModel.Name,
            UserId = httpContextService.GetContextUser().Id
        };
    }

    public static void ModifyFromUpdatingTaskList(this TaskListEntity entity, UpdateTaskListRequest businessModel)
    {
        entity.Name = businessModel.Name;
    }
}