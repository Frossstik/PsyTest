using MediatR;
using Psytest.ServiceMain.Domain.DTOs;
using Psytest.ServiceMain.Domain.Entities;

namespace Psytest.ServiceMain.Application.Commands.ProcessStaiTest
{
    public record ProcessStaiTestCommand(Guid SessionId, StaiAnswers Answers)
        : IRequest<TestResult>;
}
