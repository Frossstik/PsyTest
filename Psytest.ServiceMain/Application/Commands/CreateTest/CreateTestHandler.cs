using MediatR;
using Psytest.ServiceMain.Application.Commands.CreateSession;
using Psytest.ServiceMain.Domain.Entities;
using Psytest.ServiceMain.Infrastructure;

namespace Psytest.ServiceMain.Application.Commands.CreateTest
{
    public class CreateTestHandler : IRequestHandler<CreateTestCommand, Test>
    {
        private readonly MainDbContext _db;

        public CreateTestHandler(MainDbContext db)
        {
            _db = db;
        }

        public async Task<Test> Handle(CreateTestCommand request, CancellationToken cancellationToken)
        {

            var test = new Test
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                ShortDescription = request.ShortDescription,
                Description = request.Description,
            };

            _db.Tests.Add(test);
            await _db.SaveChangesAsync(cancellationToken);

            return test;
        }
    }
}
