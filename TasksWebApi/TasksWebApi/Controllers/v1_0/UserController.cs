using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TasksWebApi.FilterAttributes;
using TasksWebApi.Models;
using TasksWebApi.Services;

namespace TasksWebApi.Controllers.v1_0;

[ApiVersion("1.0")]
[ApiKey]
public class UserController : BaseController
{
    private readonly IUserService _service;

    public UserController(IUserService service)
    {
        _service = service;
    }
    
    [HttpPost]
    [Route("signin")]
    [ProducesResponseType(typeof(PaginationResponse<TokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SignInAsync([FromBody]SignInRequest user, CancellationToken cancellationToken = default)
    {
        if (user == null)
            return BadRequest();

        if (!ModelState.IsValid)
            return BadRequest();

        TokenResponse tokenInfo = await _service.SignInAsync(user, cancellationToken);
        return tokenInfo == null ? Unauthorized() : Ok(tokenInfo);
    }
    
    [HttpPost]
    [Route("signup")]
    [ProducesResponseType(typeof(PaginationResponse<TokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SignUpAsync([FromBody]SignUpRequest user, CancellationToken cancellationToken = default)
    {
        if (user == null)
            return BadRequest();

        if (!ModelState.IsValid)
            return BadRequest();

        TokenResponse tokenInfo = await _service.SignUpAsync(user, cancellationToken);
        return tokenInfo == null ? BadRequest() : Ok(tokenInfo);
    }
    
    [HttpPut]
    [Route("delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteAsync([FromBody]UserDeleteRequest deleteRequest, CancellationToken cancellationToken = default)
    {
        if (deleteRequest == null)
            return BadRequest();

        if (!ModelState.IsValid)
            return BadRequest();

        await _service.DeleteAsync(deleteRequest, cancellationToken);
        return NoContent();
    }
    
    [HttpPost]
    [Route("refreshToken")]
    [ProducesResponseType(typeof(PaginationResponse<TokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshTokenAsync([FromBody]RefreshTokenRequest refreshTokenRequest, CancellationToken cancellationToken = default)
    {
        if (refreshTokenRequest == null)
            return BadRequest();

        if (!ModelState.IsValid)
            return BadRequest();

        TokenResponse tokenInfo = await _service.RefreshTokenAsync(refreshTokenRequest, cancellationToken);
        return tokenInfo == null ? Unauthorized() : Ok(tokenInfo);
    }
}