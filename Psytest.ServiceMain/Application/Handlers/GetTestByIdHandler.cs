using MediatR;
using Psytest.ServiceMain.Application.Queries;
using Psytest.ServiceMain.Domain.Entities;
using Psytest.ServiceMain.Infrastructure;
using System;

namespace Psytest.ServiceMain.Application.Handlers
{
    public class GetTestByIdHandler : IRequestHandler<GetTestByIdQuery, Test?>
    {
        private readonly MainDbContext _db;
        public GetTestByIdHandler(MainDbContext db) => _db = db;

        public async Task<Test?> Handle(GetTestByIdQuery request, CancellationToken cancellationToken)
        {
            return await _db.Tests.FindAsync(new object?[] { request.Id }, cancellationToken);
        }
    }

}
