using System.ComponentModel.DataAnnotations;

namespace TasksWebApi.DataAccess.Entities;

public abstract class BaseEntity
{
    public int Id { get; set; }
    public bool IsDeleted { get; set; }
    
    [Timestamp]
    public byte[] RowVersion { get; set; }
}