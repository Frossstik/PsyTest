namespace Psytest.ServiceMain.Domain.DTOs
{
    public class TestHistoryHalDto
    {
        public object _links { get; set; } = default!;
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public IEnumerable<TestHistoryDto> Items { get; set; } = new List<TestHistoryDto>();
    }
}
