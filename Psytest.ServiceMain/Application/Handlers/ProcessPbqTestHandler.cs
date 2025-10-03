using MediatR;
using Microsoft.EntityFrameworkCore;
using Psytest.ServiceMain.Application.Commands;
using Psytest.ServiceMain.Domain.Entities;
using Psytest.ServiceMain.Domain.Logic;
using Psytest.ServiceMain.Domain.Logic.Interfaces;
using Psytest.ServiceMain.Infrastructure;

namespace Psytest.ServiceMain.Application.Handlers
{
    public class ProcessPbqTestHandler : IRequestHandler<ProcessPbqTestCommand, TestResult>
    {
        private readonly PbqTestProcessor _processor;
        private readonly MainDbContext _db;

        public ProcessPbqTestHandler(IEnumerable<ITestProcessor> processors, MainDbContext db)
        {
            _processor = processors.OfType<PbqTestProcessor>().First();
            _db = db;
        }

        public async Task<TestResult> Handle(ProcessPbqTestCommand request, CancellationToken cancellationToken)
        {
            var session = await _db.TestSessions
                .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

            if (session == null)
                throw new InvalidOperationException("Сессия не найдена");

            var result = _processor.Process(session, request.Answers);

            _db.TestResults.Add(result);
            await _db.SaveChangesAsync(cancellationToken);

            return result;
        }
    }
}
