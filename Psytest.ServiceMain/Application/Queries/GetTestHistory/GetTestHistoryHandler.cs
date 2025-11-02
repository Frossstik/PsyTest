using MediatR;
using Microsoft.EntityFrameworkCore;
using Psytest.ServiceMain.Domain.DTOs;
using Psytest.ServiceMain.Domain.Entities;
using Psytest.ServiceMain.Infrastructure;

namespace Psytest.ServiceMain.Application.Queries.GetTestHistory
{
    public class GetTestHistoryHandler : IRequestHandler<GetTestHistoryQuery, TestHistoryHalDto>
    {
        private readonly MainDbContext _db;

        public GetTestHistoryHandler(MainDbContext db)
        {
            _db = db;
        }

        public async Task<TestHistoryHalDto> Handle(GetTestHistoryQuery request, CancellationToken cancellationToken)
        {
            // 🔹 Базовый запрос
            var query = from result in _db.TestResults
                        join session in _db.TestSessions
                            on result.SessionId equals session.Id
                        join test in _db.Tests on session.TestId equals test.Id
                        where session.UserId == request.UserId
                        orderby session.CompletedAt descending
                        select new TestHistoryDto
                        {
                            SessionId = session.Id,
                            TestId = test.Id,
                            TestName = test.Name,          // <- вот!
                            ResultText = result.ResultText,
                            CompletedAt = session.CompletedAt,
                            HasReport = result.ReportBytes != null,
                            HasImages = result.Images != null && result.Images.Count > 0
                        };

            // 🔹 Подсчёт общего количества элементов
            var totalCount = await query.CountAsync(cancellationToken);
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            // 🔹 Пагинация
            var items = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            // 🔹 Формируем HAL-структуру (без прямого доступа к HttpContext — всё на уровне данных)
            string basePath = $"/api/Results/history/{request.UserId}";
            var halLinks = new
            {
                self = $"{basePath}?page={request.Page}&pageSize={request.PageSize}",
                next = request.Page < totalPages ? $"{basePath}?page={request.Page + 1}&pageSize={request.PageSize}" : null,
                prev = request.Page > 1 ? $"{basePath}?page={request.Page - 1}&pageSize={request.PageSize}" : null
            };

            // 🔹 Возвращаем DTO с HAL
            return new TestHistoryHalDto
            {
                _links = halLinks,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = totalPages,
                TotalCount = totalCount,
                Items = items
            };
        }
    }
}
