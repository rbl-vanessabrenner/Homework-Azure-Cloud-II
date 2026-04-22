using Library.API.Middleware.Auth;
using Library.API.Models;
using Library.Common.Entities;
using Library.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EndangeredAnimals.API.Controllers;

[Route("api/v2/login")]
[ApiController]
[ApiExplorerSettings(GroupName = "v2")]
public class LoginController : ControllerBase
{
    private readonly IUserService _userService;

    public LoginController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    [AllowAnonymous]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AuthenticateResponse), StatusCodes.Status200OK)]
    public IActionResult Login([FromBody] AuthenticateRequest request)
    {
        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            return BadRequest("Username and password are required.");

        var tokens = _userService.Authenticate(request.Username, request.Password);

        if (tokens == null)        
            return Unauthorized();
        
        return Ok(new AuthenticateResponse
        {
            IdToken = tokens.IdToken,
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken
        });
    }

    [HttpPost]
    [Route("refresh")]
    [AllowAnonymous]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AuthenticateResponse), StatusCodes.Status200OK)]
    public IActionResult Refresh([FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrEmpty(request.RefreshToken))
            return BadRequest("Refresh token is required.");

        var tokens = _userService.RefreshTokens(request.RefreshToken);

        if (tokens == null)
            return Unauthorized();

        return Ok(new AuthenticateResponse
        {
            IdToken = tokens.IdToken,
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken
        });
    }

    [HttpPost]
    [Route("revoke-token")]
    [Authorize]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Revoke([FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrEmpty(request.RefreshToken))
            return BadRequest("Refresh token is required.");

        var isRevoked = _userService.RevokeToken(request.RefreshToken);

        if (!isRevoked)
            return BadRequest("Token is invalid or already revoked.");

        return Ok("Token revoked successfully.");
    }

    [HttpPost]
    [Route("renew")]
    [Authorize]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(AuthenticateResponse), StatusCodes.Status200OK)]
    public IActionResult Renew()
    {
        var currentUser = HttpContext.Items["User"] as User;

        if (currentUser == null)
            return Unauthorized();

        var tokens = _userService.RenewTokens(currentUser.Email);

        if (tokens == null)
            return Unauthorized();

        return Ok(new AuthenticateResponse
        {
            IdToken = tokens.IdToken,
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken
        });
    }
}
