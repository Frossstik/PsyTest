namespace Psytest.ServiceMain.Domain.Entities
{
    public class TestResult
    {
        public Guid Id { get; set; }
        public Guid SessionId { get; set; }
        public string ResultText { get; set; } = default!; // итог для отображения
        public byte[]? ReportBytes { get; set; } // Содержимое отчёта (docx/pdf) в памяти
        public List<byte[]>? Images { get; set; } = new(); // Содержимое изображений (png/jpg) в памяти
    }
}
