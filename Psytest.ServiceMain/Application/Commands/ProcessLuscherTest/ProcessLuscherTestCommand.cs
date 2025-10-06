using MediatR;
using Psytest.ServiceMain.Domain.DTOs;
using Psytest.ServiceMain.Domain.Entities;

namespace Psytest.ServiceMain.Application.Commands.ProcessLuscherTest
{
    public record ProcessLuscherTestCommand(Guid SessionId, LuscherAnswers Answers)
        : IRequest<TestResult>;
}
