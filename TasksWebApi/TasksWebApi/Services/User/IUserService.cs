using TasksWebApi.Models;

namespace TasksWebApi.Services;

public interface IUserService
{
    Task<TokenResponse> SignUpAsync(SignUpRequest userInfo, CancellationToken cancellationToken = default);
    Task<TokenResponse> SignInAsync(SignInRequest userInfo, CancellationToken cancellationToken = default);
    Task DeleteAsync(UserDeleteRequest deleteRequest, CancellationToken cancellationToken = default);
    Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest refreshTokenRequest, CancellationToken cancellationToken = default);
}