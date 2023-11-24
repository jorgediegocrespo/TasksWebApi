using TasksWebApi.Models;

namespace TasksWebApi.Services;

public interface IHttpContextService
{
    UserResponse GetContextUser();
}