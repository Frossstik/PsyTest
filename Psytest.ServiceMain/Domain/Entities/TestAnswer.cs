namespace Psytest.ServiceMain.Domain.Entities
{
    public class TestAnswer
    {
        public Guid Id { get; set; }
        public Guid SessionId { get; set; }
        // JSON-данные, т.к. структура у разных тестов разная (у Люшера – два массива, у Равена – выбранные варианты).
        public string RawAnswers { get; set; } = default!;
    }
}
