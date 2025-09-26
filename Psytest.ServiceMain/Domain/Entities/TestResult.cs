namespace Psytest.ServiceMain.Domain.Entities
{
    public class TestResult
    {
        public Guid Id { get; set; }
        public Guid SessionId { get; set; }
        public string ResultText { get; set; } = default!; // итог для отображения
        public string? ReportPath { get; set; }           // путь к docx/pdf файлу
    }
}
