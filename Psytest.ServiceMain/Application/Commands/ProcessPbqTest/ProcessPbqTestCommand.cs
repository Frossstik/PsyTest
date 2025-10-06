using MediatR;
using Psytest.ServiceMain.Domain.DTOs;
using Psytest.ServiceMain.Domain.Entities;

namespace Psytest.ServiceMain.Application.Commands.ProcessPbqTest
{
    public record ProcessPbqTestCommand(Guid SessionId, PbqAnswers Answers)
        : IRequest<TestResult>;
}
