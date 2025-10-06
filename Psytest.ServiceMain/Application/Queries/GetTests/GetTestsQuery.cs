using MediatR;
using Psytest.ServiceMain.Domain.Entities;

namespace Psytest.ServiceMain.Application.Queries.GetTests
{
    public record class GetTestsQuery : IRequest<List<Test>>;
}
