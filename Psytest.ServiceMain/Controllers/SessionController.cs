using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Psytest.ServiceMain.Application.Commands;
using System.Security.Claims;

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

        [Authorize]
        [HttpPost("{testId:guid}/sessions")]
        public async Task<IActionResult> CreateSession(Guid testId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Невалидный токен");

            var sessionId = await _mediator.Send(new CreateTestSessionCommand(
                testId,
                Guid.Parse(userId)
            ));

            return Ok(new { SessionId = sessionId });
        }
    }

    public class CreateSessionRequest
    {
        public Guid TestId { get; set; }
    }
}
