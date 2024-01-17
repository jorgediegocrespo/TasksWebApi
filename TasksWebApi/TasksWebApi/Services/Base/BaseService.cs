using TasksWebApi.DataAccess;

namespace TasksWebApi.Services;

public abstract class BaseService(IUnitOfWork unitOfWork) : IBaseService
{
    protected readonly IUnitOfWork unitOfWork = unitOfWork;
}