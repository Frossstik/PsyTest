using MediatR;
using Microsoft.EntityFrameworkCore;
using Psytest.ServiceMain.Domain.DTOs;
using Psytest.ServiceMain.Infrastructure;

namespace Psytest.ServiceMain.Application.Queries.GetTestHistory
{
    public class GetTestHistoryHandler : IRequestHandler<GetTestHistoryQuery, List<TestHistoryDto>>
    {
        private readonly MainDbContext _db;

        public GetTestHistoryHandler(MainDbContext db)
        {
            _db = db;
        }

        public async Task<List<TestHistoryDto>> Handle(GetTestHistoryQuery request, CancellationToken cancellationToken)
        {
            var query = from result in _db.TestResults
                        join session in _db.TestSessions
                            on result.SessionId equals session.Id
                        where session.UserId == request.UserId
                        orderby session.CompletedAt descending
                        select new TestHistoryDto
                        {
                            SessionId = session.Id,
                            ResultText = result.ResultText,
                            CompletedAt = session.CompletedAt,
                            HasReport = result.ReportBytes != null,
                            HasImages = result.Images != null && result.Images.Count > 0
                        };

            return await query.ToListAsync(cancellationToken);
        }
    }
}
