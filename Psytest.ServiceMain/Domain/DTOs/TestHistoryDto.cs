namespace Psytest.ServiceMain.Domain.DTOs
{
    public class TestHistoryDto
    {
        public Guid SessionId { get; set; }
        public Guid TestId { get; set; }           // <- удобно для ссылок на описание теста
        public string TestName { get; set; } = ""; // <- НАЗВАНИЕ ТЕСТА
        public string ResultText { get; set; } = "";
        public DateTime? CompletedAt { get; set; }
        public bool HasReport { get; set; }
        public bool HasImages { get; set; }
    }
}
