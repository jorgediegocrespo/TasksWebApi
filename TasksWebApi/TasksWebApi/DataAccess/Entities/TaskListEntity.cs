namespace TasksWebApi.DataAccess.Entities;

public sealed class TaskListEntity : BaseEntity
{
    public string Name { get; set; }
    public List<TaskEntity> Tasks { get; set; }
    public string UserId { get; set; }
    public UserEntity User { get; set; }
}