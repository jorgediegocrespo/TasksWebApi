using System.ComponentModel.DataAnnotations;

namespace TasksWebApi.Models;

public record DeleteRequest
{
    [Required]
    public int Id { get; init; }
    
    [Required]
    public byte[] RowVersion { get; init; }

    public DeleteRequest(int id, byte[] rowVersion)
    {
        Id = id;
        RowVersion = rowVersion;
    }
}