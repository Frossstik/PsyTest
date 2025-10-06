using MediatR;
using Microsoft.EntityFrameworkCore;
using Psytest.ServiceMain.Domain.Entities;
using Psytest.ServiceMain.Infrastructure;

namespace Psytest.ServiceMain.Application.Queries.GetTests
{
    public class GetTestsHandler : IRequestHandler<GetTestsQuery, List<Test>>
    {
        private readonly MainDbContext _db;

        public GetTestsHandler(MainDbContext db)
        {
            _db = db;
        }
        public async Task<List<Test>> Handle(GetTestsQuery request, CancellationToken cancellationToken)
        {
            var tests = await _db.Tests.ToListAsync(cancellationToken);
            return tests;
        }
    }
}
