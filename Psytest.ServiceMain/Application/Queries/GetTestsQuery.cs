using MediatR;
using Psytest.ServiceMain.Domain.Entities;

namespace Psytest.ServiceMain.Application.Queries
{
    public record class GetTestsQuery : IRequest<List<Test>>;
}
