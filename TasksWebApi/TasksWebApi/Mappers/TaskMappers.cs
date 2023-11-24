using TasksWebApi.DataAccess.Entities;
using TasksWebApi.Models;

namespace TasksWebApi.Mappers;

public static class TaskMappers
{
    public static ReadTaskResponse ToReadingTask(this TaskEntity entity)
    {
        return entity == null ? 
            null : 
            new ReadTaskResponse(entity.Id, entity.RowVersion, entity.TaskListId, entity.Description, entity.Notes);
    }

    public static TaskEntity ToTaskEntity(this CreateTaskRequest businessModel)
    {
        return new TaskEntity()
        {
            TaskListId = businessModel.TaskListId,
            Description = businessModel.Description,
            Notes = businessModel.Notes
        };
    }

    public static void ModifyFromUpdatingTask(this TaskEntity entity, UpdateTaskRequest businessModel)
    {
        entity.TaskListId = businessModel.TaskListId;
        entity.Description = businessModel.Description;
        entity.Notes = businessModel.Notes;
    }
}