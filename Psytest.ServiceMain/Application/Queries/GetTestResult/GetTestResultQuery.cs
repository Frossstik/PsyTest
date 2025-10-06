using MediatR;
using Psytest.ServiceMain.Domain.Entities;

namespace Psytest.ServiceMain.Application.Queries.GetTestResult
{
    public record GetTestResultQuery(Guid SessionId) : IRequest<TestResult?>;
}
