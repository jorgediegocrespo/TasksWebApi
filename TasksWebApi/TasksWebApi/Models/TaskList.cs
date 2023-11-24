using System.ComponentModel.DataAnnotations;

namespace TasksWebApi.Models;

public record ReadTaskListResponse
{
    public int Id { get; init; }
    public byte[] RowVersion { get; init; }
    public string Name { get; init; }
    
    public ReadTaskListResponse(int id, byte[] rowVersion, string name)
    {
        Id = id;
        RowVersion = rowVersion;
        Name = name;
    }
}

public record CreateTaskListRequest
{
    [Required]
    [MaxLength(50)]
    [MinLength(4)]
    public string Name { get; init; }
    
    public CreateTaskListRequest(string name)
    {
        Name = name;
    }
}

public record UpdateTaskListRequest
{
    [Required]
    public int Id { get; init; }
    
    [Required]
    public byte[] RowVersion { get; init; }

    [Required]
    [MaxLength(50)]
    [MinLength(4)]
    public string Name { get; init; }
    
    public UpdateTaskListRequest(int id, byte[] rowVersion, string name)
    {
        Id = id;
        RowVersion = rowVersion;
        Name = name;
    }
}