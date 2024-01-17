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
public class UserController(IUserService service) : BaseController
{
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

        var tokenInfo = await service.SignInAsync(user, cancellationToken);
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

        var tokenInfo = await service.SignUpAsync(user, cancellationToken);
        return tokenInfo == null ? BadRequest() : Ok(tokenInfo);
    }
    
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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

        await service.DeleteAsync(deleteRequest, cancellationToken);
        return NoContent();
    }
    
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Roles.SUPERADMIN)]
    [HttpPut]
    [Route("deleteWithData")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteWithDataAsync([FromBody]UserDeleteRequest deleteRequest, CancellationToken cancellationToken = default)
    {
        if (deleteRequest == null)
            return BadRequest();

        if (!ModelState.IsValid)
            return BadRequest();

        await service.DeleteWithDataAsync(deleteRequest, cancellationToken);
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

        var tokenInfo = await service.RefreshTokenAsync(refreshTokenRequest, cancellationToken);
        return tokenInfo == null ? Unauthorized() : Ok(tokenInfo);
    }
}