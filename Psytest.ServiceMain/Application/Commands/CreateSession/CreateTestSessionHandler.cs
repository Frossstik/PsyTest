using MediatR;
using Psytest.ServiceMain.Domain.Entities;
using Psytest.ServiceMain.Infrastructure;
using Psytest.ServiceMain.Infrastructure.Grpc;

namespace Psytest.ServiceMain.Application.Commands.CreateSession
{
    public class CreateTestSessionHandler : IRequestHandler<CreateTestSessionCommand, Guid>
    {
        private readonly MainDbContext _db;
        private readonly IdentityGrpcClient _identityGrpcClient;

        public CreateTestSessionHandler(IdentityGrpcClient identityGrpcClient, MainDbContext db)
        {
            _identityGrpcClient = identityGrpcClient;
            _db = db;
        }

        public async Task<Guid> Handle(CreateTestSessionCommand request, CancellationToken cancellationToken)
        {

            var session = new TestSession
            {
                Id = Guid.NewGuid(),
                TestId = request.TestId,
                UserId = request.UserId,
                StartedAt = DateTime.UtcNow,
                Status = "InProgress"
            };

            _db.TestSessions.Add(session);
            await _db.SaveChangesAsync(cancellationToken);

            return session.Id;
        }
    }
}
