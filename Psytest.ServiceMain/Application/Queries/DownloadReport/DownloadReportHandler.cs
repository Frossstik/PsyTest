using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Psytest.ServiceMain.Infrastructure;

namespace Psytest.ServiceMain.Application.Queries.DownloadReport
{
    public class DownloadReportHandler : IRequestHandler<DownloadReportQuery, FileResult>
    {
        private readonly MainDbContext _db;

        public DownloadReportHandler(MainDbContext db)
        {
            _db = db;
        }

        public async Task<FileResult> Handle(DownloadReportQuery request, CancellationToken cancellationToken)
        {
            var result = await _db.TestResults
                .FirstOrDefaultAsync(r => r.SessionId == request.SessionId, cancellationToken);

            if (result == null || result.ReportBytes == null || result.ReportBytes.Length == 0)
                throw new FileNotFoundException("Файл отчёта не найден");

            return new FileContentResult(result.ReportBytes,
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
            {
                FileDownloadName = $"report_{request.SessionId}.docx"
            };
        }
    }
}
