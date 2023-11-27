using System.ComponentModel.DataAnnotations;

namespace TasksWebApi.DataAccess.Entities;

public abstract class BaseEntity
{
    public int Id { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime DateOfCreation { get; set; }
    public string CreationUser { get; set; }
    public DateTime? DateOfLastUpdate { get; set; }
    public string LastUpdateUser { get; set; }
    public DateTime? DateOfDelete { get; set; }
    public string DeleteUser { get; set; }
    
    [Timestamp]
    public byte[] RowVersion { get; set; }
}