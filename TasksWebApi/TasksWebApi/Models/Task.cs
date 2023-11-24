using System.ComponentModel.DataAnnotations;

namespace TasksWebApi.Models;

public record TaskPaginationRequest : PaginationRequest
{
    public int TaskListId { get; init; }
    
    public TaskPaginationRequest(int taskListId, int pageSize, int pageNumber) : base(pageSize, pageNumber)
    {
        TaskListId = taskListId;
    }
}

public record ReadTaskResponse
{
    public int Id { get; init; }
    public byte[] RowVersion { get; init; }
    public int TaskListId { get; init; }
    public string Description { get; init; }
    public string? Notes { get; init; }
    
    public ReadTaskResponse(int id, byte[] rowVersion, int taskListId, string description, string? notes)
    {
        Id = id;
        RowVersion = rowVersion;
        TaskListId = taskListId;
        Description = description;
        Notes = notes;
    }
}

public record CreateTaskRequest
{
    [Required]
    public int TaskListId { get; init; }
    
    [Required]
    [MaxLength(50)]
    [MinLength(4)]
    public string Description { get; init; }
    
    public string? Notes { get; init; }
    
    public CreateTaskRequest(int taskListId, string description, string? notes)
    {
        TaskListId = taskListId;
        Description = description;
        Notes = notes;
    }
}

public record UpdateTaskRequest
{
    [Required]
    public int Id { get; init; }
    
    [Required]
    public byte[] RowVersion { get; init; }
    
    [Required]
    public int TaskListId { get; init; }

    [Required]
    [MaxLength(50)]
    [MinLength(4)]
    public string Description { get; init; }
    
    public string? Notes { get; init; }
    
    public UpdateTaskRequest(int id, byte[] rowVersion, int taskListId, string description, string? notes)
    {
        Id = id;
        RowVersion = rowVersion;
        TaskListId = taskListId;
        Description = description;
        Notes = notes;
    }
}