using Microsoft.EntityFrameworkCore;
using Psytest.ServiceMain.Domain.Entities;
using static System.Net.Mime.MediaTypeNames;

namespace Psytest.ServiceMain.Infrastructure
{
    public class MainDbContext : DbContext
    {
        private readonly IConfiguration _configuration;
        public MainDbContext(DbContextOptions options, IConfiguration configuration)
        : base(options)
        {
            _configuration = configuration;
        }

        public DbSet<Test> Tests { get; set; } = default!;
        public DbSet<TestSession> TestSessions { get; set; } = default!;
        public DbSet<TestAnswer> TestAnswers { get; set; } = default!;
        public DbSet<TestResult> TestResults { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Test>().HasData(new Test
            {
                Id = Guid.Parse(_configuration["SeedData:LuscherTestGuid"]),
                Name = "Тест Люшера",
                ShortDescription = "Психологический цветовой тест Макса Люшера",
                Description = "Психологический тест, " +
                "разработанный Максом Люшером, " +
                "основан на идее о том, " +
                "что цветовые предпочтения могут раскрыть " +
                "эмоциональное состояние и черты характера человека."
            });

            modelBuilder.Entity<Test>(e =>
            {
                e.HasKey(t => t.Id);
                e.Property(t => t.Name).IsRequired();
            });

            modelBuilder.Entity<TestSession>(e =>
            {
                e.HasKey(ts => ts.Id);
            });

            modelBuilder.Entity<TestAnswer>(e =>
            {
                e.HasKey(a => a.Id);
            });

            modelBuilder.Entity<TestResult>(e =>
            {
                e.HasKey(r => r.Id);
            });
        }
    }
}
