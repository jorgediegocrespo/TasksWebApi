using System.ComponentModel.DataAnnotations;

namespace TasksWebApi.Models;

public record PaginationRequest
{
    [Required]
    [Range(1, 100)]
    public int PageSize { get; init; }

    [Required]
    public int PageNumber { get; init; }
    
    public PaginationRequest(int pageSize, int pageNumber)
    {
        PageSize = pageSize;
        PageNumber = pageNumber;
    }
}

public record PaginationResponse<T>
{
    public int TotalRegisters { get; init; }
    public IEnumerable<T> Result { get; init; }
    
    public PaginationResponse(int totalRegisters, IEnumerable<T> result)
    {
        TotalRegisters = totalRegisters;
        Result = result;
    }
}
