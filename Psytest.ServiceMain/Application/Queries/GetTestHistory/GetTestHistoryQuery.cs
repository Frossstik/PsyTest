using MediatR;
using Psytest.ServiceMain.Domain.DTOs;
using Psytest.ServiceMain.Domain.Entities;

namespace Psytest.ServiceMain.Application.Queries.GetTestHistory
{
    public record GetTestHistoryQuery(Guid UserId) : IRequest<List<TestHistoryDto>>;
}
