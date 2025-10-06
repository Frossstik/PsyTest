using MediatR;
using Psytest.ServiceMain.Domain.DTOs;
using Psytest.ServiceMain.Domain.Entities;

namespace Psytest.ServiceMain.Application.Commands.ProcessSchmieschekTest
{
    public record ProcessSchmieschekTestCommand(Guid SessionId, SchmieschekAnswers Answers)
        : IRequest<TestResult>;
}
