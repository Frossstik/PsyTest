using MediatR;

namespace Psytest.ServiceMain.Application.Commands
{
    public record CreateTestSessionCommand(Guid TestId, Guid UserId) : IRequest<Guid>;
}
