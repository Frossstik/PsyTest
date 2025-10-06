namespace Psytest.ServiceMain.Domain.DTOs
{
    public class TestHistoryDto
    {
        public Guid SessionId { get; set; }
        public string ResultText { get; set; } = default!;
        public DateTime? CompletedAt { get; set; }
        public bool HasReport { get; set; }
        public bool HasImages { get; set; }
    }
}
