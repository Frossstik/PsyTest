using MediatR;
using Psytest.ServiceMain.Domain.Entities;

namespace Psytest.ServiceMain.Application.Queries
{
    public record GetTestResultQuery(Guid SessionId) : IRequest<TestResult?>;
}
