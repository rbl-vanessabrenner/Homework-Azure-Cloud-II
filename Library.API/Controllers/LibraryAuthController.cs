using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Library.API.Middleware.Auth;
using Library.API.Models;
using Library.API.Utils;
using Library.Common.Entities;
using Library.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [Route("api/v2/books")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v2")]
    public class LibraryAuthController : ControllerBase
    {
        private IBookServiceAsync _bookService;

        public LibraryAuthController(IBookServiceAsync bookService)
        {
            _bookService = bookService;
        }

        [HttpGet]
        [Route("{id}")]
        [Authorize(Policy = Policies.All)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BookModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest("Id cannot be empty.");

            var result = await _bookService.GetById(id);
            
            if(result == null)
                return NotFound();

            return Ok(result.ToModel());
        }

        [HttpGet]
        [Authorize(Policy = Policies.All)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(List<BookModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var results = await _bookService.Get(pageNumber, pageSize);

            if (!results.Any())
                return NoContent();

            return Ok(results.Select(x => x.ToModel()));
        }

        [HttpPost]
        [Authorize(Policy = Policies.Admin)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BookModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> Add([FromBody] BookModel model)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentUser = HttpContext.Items["User"] as User;

            if (currentUser == null)
                return Unauthorized();

            var result = await _bookService.Add(model.ToDto(null, currentUser.Id));

            return Ok(result.ToModel());
        }

        [HttpPut]
        [Route("{id}")]
        [Authorize(Policy = Policies.Admin)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BookModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] BookModel model)
        {
            if(id == Guid.Empty)
                return BadRequest("Id cannot be empty.");

            if(id != model.Id)
                return BadRequest("Ids do not match.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _bookService.Update(model.ToDto(id));

            return result != null ? Ok(result.ToModel()) : NotFound();
        }

        [HttpDelete]
        [Route("{id}")]
        [Authorize(Policy = Policies.Admin)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest("Id cannot be empty.");

            var result = await _bookService.Remove(id);

            return result ? Ok("Entity deleted succesfully.") : NotFound("Entity not found.");
        }
    }
}
