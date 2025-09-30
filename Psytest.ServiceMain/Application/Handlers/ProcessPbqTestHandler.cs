using MediatR;
using Psytest.ServiceMain.Application.Commands;
using Psytest.ServiceMain.Domain.Entities;
using Psytest.ServiceMain.Domain.Logic;

namespace Psytest.ServiceMain.Application.Handlers
{
    public class ProcessPbqTestHandler : IRequestHandler<ProcessPbqTestCommand, TestResult>
    {
        private readonly PbqTestProcessor _processor;

        public ProcessPbqTestHandler(IEnumerable<ITestProcessor> processors)
        {
            _processor = processors.OfType<PbqTestProcessor>().First();
        }

        public Task<TestResult> Handle(ProcessPbqTestCommand request, CancellationToken cancellationToken)
        {
            var session = new TestSession { Id = request.SessionId };
            var result = _processor.Process(session, request.Answers);

            return Task.FromResult(result);
        }
    }

}
