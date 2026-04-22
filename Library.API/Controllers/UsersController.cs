using Library.API.Middleware.Auth;
using Library.API.Models;
using Library.API.Utils;
using Library.Common.Entities;
using Library.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EndangeredAnimals.API.Controllers;

[Route("api/v2/users")]
[ApiController]
[ApiExplorerSettings(GroupName = "v2")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [Authorize(Policy = Policies.Admin)]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(List<UserModel>), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        var users = _userService.GetAll();

        if (users == null)
            return NoContent();

        var userModels = users.Select(user => user.ToModel()).ToList();

        return Ok(userModels);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = Policies.Admin)]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(UserModel), StatusCodes.Status200OK)]
    public IActionResult GetById(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest("Id cannot be empty.");

        var user = _userService.GetById(id);

        if (user == null)
            return NotFound();

        return Ok(user.ToModel());
    }

    [HttpGet("{id}/refresh-tokens")]
    [Authorize(Policy = Policies.Admin)]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(List<RefreshTokenEntry>), StatusCodes.Status200OK)]
    public IActionResult GetRefreshTokens(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest("Id cannot be empty.");

        var tokens = _userService.GetRefreshTokensByUserId(id);

        if (tokens == null || !tokens.Any())
            return NoContent();

        return Ok(tokens);
    }
}
