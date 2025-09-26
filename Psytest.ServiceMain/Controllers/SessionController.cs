using MediatR;
using Microsoft.AspNetCore.Mvc;
using Psytest.ServiceMain.Application.Commands;

namespace Psytest.ServiceMain.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SessionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SessionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateSession([FromBody] CreateSessionRequest request)
        {
            var sessionId = await _mediator.Send(new CreateTestSessionCommand(request.TestId, request.UserId));
            return Ok(new { SessionId = sessionId });
        }
    }

    public class CreateSessionRequest
    {
        public Guid TestId { get; set; }
        public Guid UserId { get; set; }
    }
}
