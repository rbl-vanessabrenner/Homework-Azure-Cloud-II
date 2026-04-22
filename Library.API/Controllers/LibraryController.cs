using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Library.API.Models;
using Library.API.Utils;
using Library.Common.Entities;
using Library.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [Route("api/books")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v1")]
    public class LibraryController : ControllerBase
    {
        private IBookServiceAsync _bookService;

        public LibraryController(IBookServiceAsync bookService)
        {
            _bookService = bookService;
        }

        [HttpGet]
        [Route("{id}")]
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
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BookModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> Add([FromBody] BookModel model)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _bookService.Add(model.ToDto(null));

            return Ok(result.ToModel());
        }

        [HttpPut]
        [Route("{id}")]
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
