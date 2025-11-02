using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Psytest.ServiceMain.Application.Queries;
using Psytest.ServiceMain.Application.Queries.DownloadReport;
using Psytest.ServiceMain.Application.Queries.GetTestHistory;
using Psytest.ServiceMain.Application.Queries.GetTestResult;
using Psytest.ServiceMain.Infrastructure;

namespace Psytest.ServiceMain.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResultsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ResultsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize]
        [HttpGet("results/{sessionId:guid}")]
        public async Task<IActionResult> GetResult(Guid sessionId)
        {
            var result = await _mediator.Send(new GetTestResultQuery(sessionId));

            if (result == null)
                return NotFound("Результат не найден");

            return Ok(new
            {
                result.Id,
                result.SessionId,
                result.ResultText,
                result.ReportBytes,
                result.Images
            });
        }


        [Authorize]
        [HttpGet("reports/{sessionId:guid}")]
        public async Task<IActionResult> DownloadReport(Guid sessionId)
        {
            var file = await _mediator.Send(new DownloadReportQuery(sessionId));
            return file;
        }

        [Authorize]
        [HttpGet("history/{userId:guid}")]
        public async Task<IActionResult> GetHistory(Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 5)
        {
            var hal = await _mediator.Send(new GetTestHistoryQuery(userId, page, pageSize));
            return Ok(hal);
        }
    }
}
