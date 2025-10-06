using MediatR;
using Microsoft.EntityFrameworkCore;
using Psytest.ServiceMain.Application.Commands.ProcessSchmieschekTest;
using Psytest.ServiceMain.Domain.Entities;
using Psytest.ServiceMain.Domain.Logic;
using Psytest.ServiceMain.Domain.Logic.Interfaces;
using Psytest.ServiceMain.Infrastructure;

namespace Psytest.ServiceMain.Application.Commands.ProcessPbqTest
{
    public class ProcessPbqTestHandler : IRequestHandler<ProcessPbqTestCommand, TestResult>
    {
        private readonly MainDbContext _db;
        private readonly PbqTestProcessor _processor;

        public ProcessPbqTestHandler(MainDbContext db, IEnumerable<ITestProcessor> processors)
        {
            _db = db;
            _processor = processors.OfType<PbqTestProcessor>().First();
        }

        public async Task<TestResult> Handle(ProcessPbqTestCommand request, CancellationToken cancellationToken)
        {
            var session = await _db.TestSessions.FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

            if (session == null)
                throw new InvalidOperationException("Сессия не найдена");

            // Сохраняем ответы
            var testAnswer = new TestAnswer
            {
                Id = Guid.NewGuid(),
                SessionId = request.SessionId,
                RawAnswers = System.Text.Json.JsonSerializer.Serialize(request.Answers)
            };
            _db.TestAnswers.Add(testAnswer);
            await _db.SaveChangesAsync(cancellationToken);

            // Запускаем обработку
            var result = _processor.Process(session, request.Answers);

            _db.TestResults.Add(result);
            session.CompletedAt = DateTime.UtcNow;
            session.Status = "Completed";

            await _db.SaveChangesAsync(cancellationToken);

            return result;
        }
    }
}
