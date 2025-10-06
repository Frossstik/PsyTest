using MediatR;
using Psytest.ServiceMain.Domain.Entities;

namespace Psytest.ServiceMain.Application.Queries.GetTestById
{
    public record GetTestByIdQuery(Guid Id) : IRequest<Test?>;

}
