using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Psytest.ServiceMain.Migrations
{
    /// <inheritdoc />
    public partial class CreatePbqTest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Tests",
                columns: new[] { "Id", "Description", "Name", "ShortDescription" },
                values: new object[] { new Guid("f49ef84b-8d71-4d0e-94ee-24e09fbeeca0"), "Психологический тест, разработанный для оценки поведенческих качеств человека, основан на анализе типичных моделей поведения в различных ситуациях. Тест помогает выявить личностные особенности и адаптивность к окружающей среде.", "Тест PBQ", "Психологический тест поведенческих качеств (PBQ)" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: new Guid("f49ef84b-8d71-4d0e-94ee-24e09fbeeca0"));
        }
    }
}
