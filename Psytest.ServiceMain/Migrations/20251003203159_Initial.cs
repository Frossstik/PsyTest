using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Psytest.ServiceMain.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TestAnswers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    RawAnswers = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestAnswers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TestResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResultText = table.Column<string>(type: "text", nullable: false),
                    ReportBytes = table.Column<byte[]>(type: "bytea", nullable: true),
                    Images = table.Column<List<byte[]>>(type: "bytea[]", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestResults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ShortDescription = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TestSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TestId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestSessions", x => x.Id);
                });

            //migrationBuilder.InsertData(
            //    table: "Tests",
            //    columns: new[] { "Id", "Description", "Name", "ShortDescription" },
            //    values: new object[,]
            //    {
            //        { new Guid("d22530e2-9c20-4365-9265-4bb6c9b8e8c5"), "Психологический тест, разработанный Максом Люшером, основан на идее о том, что цветовые предпочтения могут раскрыть эмоциональное состояние и черты характера человека.", "Тест Люшера", "Психологический цветовой тест Макса Люшера" },
            //        { new Guid("f49ef84b-8d71-4d0e-94ee-24e09fbeeca0"), "Психологический тест, разработанный для оценки поведенческих качеств человека, основан на анализе типичных моделей поведения в различных ситуациях. Тест помогает выявить личностные особенности и адаптивность к окружающей среде.", "Тест PBQ", "Психологический тест поведенческих качеств (PBQ)" }
            //    });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TestAnswers");

            migrationBuilder.DropTable(
                name: "TestResults");

            migrationBuilder.DropTable(
                name: "Tests");

            migrationBuilder.DropTable(
                name: "TestSessions");
        }
    }
}
