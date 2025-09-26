using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Psytest.ServiceMain.Application.Commands;
using Psytest.ServiceMain.Application.Queries;
using Psytest.ServiceMain.Domain.Entities;

namespace Psytest.ServiceMain.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TestsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<List<Test>>> Get()
        {
            var tests = await _mediator.Send(new GetTestsQuery());
            return Ok(tests);
        }

        /// <summary>
        /// Обработать тест Люшера
        /// </summary>
        [HttpPost("{sessionId:guid}/luscher")]
        public async Task<ActionResult<TestResult>> ProcessLuscherTest(Guid sessionId, [FromBody] ProcessLuscherTestCommand command)
        {
            if (sessionId != command.SessionId)
                return BadRequest("SessionId не совпадает");

            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
