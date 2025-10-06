using MediatR;
using Psytest.ServiceMain.Domain.Entities;

namespace Psytest.ServiceMain.Application.Commands.CreateTest
{
    public record CreateTestCommand(string Name, string? ShortDescription, string? Description)
        : IRequest<Test>;
}
