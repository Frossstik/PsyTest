using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Psytest.ServiceMain.Application.Commands;
using Psytest.ServiceMain.Application.Queries;
using Psytest.ServiceMain.Domain.DTOs;
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

        [HttpPost("{sessionId:guid}/luscher")]
        public async Task<ActionResult<TestResult>> ProcessLuscherTest(Guid sessionId, [FromBody] LuscherAnswers answers)
        {
            var command = new ProcessLuscherTestCommand(sessionId, answers);
            var result = await _mediator.Send(command);

            return Ok(result);
        }

        [HttpPost("{sessionId:guid}/pbq")]
        public async Task<ActionResult<TestResult>> ProcessPbqTest(Guid sessionId, [FromBody] PbqAnswers answers)
        {
            var command = new ProcessPbqTestCommand(sessionId, answers);
            var result = await _mediator.Send(command);

            return Ok(result);
        }
    }
}
