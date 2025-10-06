using MediatR;
using Microsoft.EntityFrameworkCore;
using Psytest.ServiceMain.Domain.Entities;
using Psytest.ServiceMain.Infrastructure;

namespace Psytest.ServiceMain.Application.Queries.GetTestResult
{
    public class GetTestResultHandler : IRequestHandler<GetTestResultQuery, TestResult?>
    {
        private readonly MainDbContext _db;

        public GetTestResultHandler(MainDbContext db)
        {
            _db = db;
        }

        public async Task<TestResult?> Handle(GetTestResultQuery request, CancellationToken cancellationToken)
        {
            return await _db.TestResults
                .FirstOrDefaultAsync(r => r.SessionId == request.SessionId, cancellationToken);
        }
    }
}
