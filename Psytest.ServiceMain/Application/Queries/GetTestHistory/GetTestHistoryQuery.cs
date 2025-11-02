using MediatR;
using Psytest.ServiceMain.Domain.DTOs;

namespace Psytest.ServiceMain.Application.Queries.GetTestHistory
{
    public record GetTestHistoryQuery(Guid UserId, int Page = 1, int PageSize = 5)
        : IRequest<TestHistoryHalDto>;
}
