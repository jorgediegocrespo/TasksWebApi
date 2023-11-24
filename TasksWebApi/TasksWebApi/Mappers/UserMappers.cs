using TasksWebApi.DataAccess.Entities;
using TasksWebApi.Models;

namespace TasksWebApi.Mappers;

public static class UserMappers
{
    public static UserEntity ToIdentityUser(this SignUpRequest userInfo)
    {
        return new UserEntity
        {
            UserName = userInfo.UserName,
            Email = userInfo.Email,
        };
    }
}