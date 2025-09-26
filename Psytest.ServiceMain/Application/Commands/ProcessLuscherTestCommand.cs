using MediatR;
using Psytest.ServiceMain.Domain.DTOs;
using Psytest.ServiceMain.Domain.Entities;

namespace Psytest.ServiceMain.Application.Commands
{
    public class ProcessLuscherTestCommand : IRequest<TestResult>
    {
        public Guid SessionId { get; set; }
        public LuscherAnswers Answers { get; set; } = default!;
    }
}
