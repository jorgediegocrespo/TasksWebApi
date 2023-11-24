namespace TasksWebApi.DataAccess.Entities;

public sealed class TaskEntity : BaseEntity
{
    public string Description { get; set; }
    public string Notes { get; set; }
    public int TaskListId { get; set; }
    public TaskListEntity TaskList { get; set; }
}