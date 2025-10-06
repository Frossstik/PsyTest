using MediatR;

namespace Psytest.ServiceMain.Application.Commands.CreateSession
{
    public record CreateTestSessionCommand(Guid TestId, Guid UserId) : IRequest<Guid>;

}
