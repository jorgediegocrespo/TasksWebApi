using TasksWebApi.DataAccess;

namespace TasksWebApi.Services;

public abstract class BaseService : IBaseService
{
    protected readonly IUnitOfWork unitOfWork;
    
    public BaseService(IUnitOfWork unitOfWork)
    {
        this.unitOfWork = unitOfWork;
    }
}