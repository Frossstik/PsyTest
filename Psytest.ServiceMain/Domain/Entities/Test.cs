namespace Psytest.ServiceMain.Domain.Entities
{
    public class Test
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? ShortDescription { get; set; }
        public string? Description { get; set; }
    }
}
