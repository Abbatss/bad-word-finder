using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using BWF.Api.Host.Models;
using BWF.Api.Services.Store;
using BWF.Api.Services.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace BWF.Api.Host.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BadWordsController : ControllerBase
    {
        private readonly IBadWordsRepository _repository;

        public BadWordsController(IBadWordsRepository repository)
        {
            _repository = repository ?? throw new System.ArgumentNullException(nameof(repository));
        }

        /// <summary>
        /// Get a bad word by Id.
        /// </summary>
        /// <param name="id">Id of a bad word.</param>
        /// <returns>Bad Word model.</returns>
        /// <response code="200">The bad word is found.</response>
        /// <response code="400">>No Id specified.</response>
        /// <response code="401">Unauthorized access, no access token provided by a client.</response>
        /// <response code="404">The bad word is not found by Id.</response>
        /// <response code="500">Unhandled server error.</response>
        [HttpGet("{id}", Name = nameof(BadWord))]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(BadWord), 200)]
        [ProducesResponseType(typeof(void), 202)]
        [ProducesResponseType(typeof(void), 400)]
        [ProducesResponseType(typeof(void), 401)]
        [ProducesResponseType(typeof(void), 404)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<ActionResult> Get(string id)
        {
            var res = await _repository.GetAsync(id).ConfigureAwait(false);
            if (res == null)
            {
                return NotFound();
            }

            return Ok(res);
        }
        [HttpGet]
        public async Task<ActionResult> GetList()
        {
            var res =  await _repository.GetAllAsync();
            return Ok(res);

        }

        /// <summary>
        /// Create new bad word. Word will be stored as lower invariant
        /// </summary>
        /// <param name="request"> </param>
        /// <returns>201 - Created and bad word object.</returns>
        /// <response code="201">bad word created successfully.</response>
        /// <response code="400">Missed parameters.</response>
        /// <response code="401">Unauthorized access, no access token provided by a client.</response>
        /// <response code="500">Unhandled server error.</response>
        [HttpPost]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(BadWord), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.InternalServerError)]
        public async Task<BadWord> Add([Required] AddBadWordRequest request)
        {

            var badWord = new BadWord() { Id = Guid.NewGuid().ToString(), Word = request.Word.ToLowerInvariant() };

            return await _repository.AddAsync(badWord).ConfigureAwait(false);
        }

        /// <summary>
        /// Removes a bad word by Id.
        /// </summary>
        /// <param name="id">The unique Id of a bad word to remove.</param>
        /// <response code="200">The requested bad word is removed.</response>
        /// <response code="400">No Id specified.</response>
        /// <response code="401">Unauthorized access, no access token provided by a client.</response>
        /// <response code="500">Unhandled server error.</response>
        /// <returns>HTTP status of the delete operation.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(void), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(typeof(void), 401)]
        [ProducesResponseType(typeof(void), 404)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task Delete(string id)
        {
            await _repository.DeleteAsync(id).ConfigureAwait(false);
        }
    }
}
