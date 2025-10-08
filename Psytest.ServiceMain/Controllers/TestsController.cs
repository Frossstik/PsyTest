using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Core.Types;
using Psytest.ServiceMain.Application.Commands;
using Psytest.ServiceMain.Application.Commands.CreateTest;
using Psytest.ServiceMain.Application.Commands.ProcessLuscherTest;
using Psytest.ServiceMain.Application.Commands.ProcessPbqTest;
using Psytest.ServiceMain.Application.Commands.ProcessSchmieschekTest;
using Psytest.ServiceMain.Application.Commands.ProcessStaiTest;
using Psytest.ServiceMain.Application.Queries;
using Psytest.ServiceMain.Application.Queries.GetTestById;
using Psytest.ServiceMain.Application.Queries.GetTests;
using Psytest.ServiceMain.Domain.DTOs;
using Psytest.ServiceMain.Domain.Entities;
using ScottPlot.TickGenerators.Financial;

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

        [HttpGet("{id:Guid}")]
        public async Task<ActionResult<Test>> GetById(Guid id)
        {
            var test = await _mediator.Send(new GetTestByIdQuery(id));
            if (test == null)
                return NotFound();

            return Ok(test);
        }

        [HttpPost]
        public async Task<ActionResult<Test>> CreateTest([FromBody] CreateTestCommand command)
        {
            var created = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
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

        [HttpPost("{sessionId:guid}/schmieschek")]
        public async Task<ActionResult<TestResult>> ProcessSchmieschekTest(Guid sessionId, [FromBody] SchmieschekAnswers answers)
        {
            var command = new ProcessSchmieschekTestCommand(sessionId, answers);
            var result = await _mediator.Send(command);

            return Ok(result);
        }

        [HttpPost("{sessionId:guid}/stai")]
        public async Task<ActionResult<TestResult>> ProcessStaiTest(Guid sessionId, [FromBody] StaiAnswers answers)
        {
            var command = new ProcessStaiTestCommand(sessionId, answers);
            var result = await _mediator.Send(command);

            return Ok(result);
        }
    }
}
