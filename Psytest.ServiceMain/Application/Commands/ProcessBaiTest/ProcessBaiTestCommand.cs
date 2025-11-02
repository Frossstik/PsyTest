using MediatR;
using Psytest.ServiceMain.Domain.DTOs;
using Psytest.ServiceMain.Domain.Entities;

namespace Psytest.ServiceMain.Application.Commands.ProcessBaiTest
{
    public record ProcessBaiTestCommand(Guid SessionId, BaiAnswers Answers)
        : IRequest<TestResult>;
}
