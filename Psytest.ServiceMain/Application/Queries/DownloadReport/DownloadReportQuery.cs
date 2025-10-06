using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Psytest.ServiceMain.Application.Queries.DownloadReport
{
    public record DownloadReportQuery(Guid SessionId) : IRequest<FileResult>;
}
