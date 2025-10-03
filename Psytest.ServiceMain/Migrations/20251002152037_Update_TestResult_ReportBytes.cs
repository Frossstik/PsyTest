using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Psytest.ServiceMain.Migrations
{
    /// <inheritdoc />
    public partial class Update_TestResult_ReportBytes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReportPath",
                table: "TestResults");

            migrationBuilder.AddColumn<byte[]>(
                name: "ReportBytes",
                table: "TestResults",
                type: "bytea",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReportBytes",
                table: "TestResults");

            migrationBuilder.AddColumn<string>(
                name: "ReportPath",
                table: "TestResults",
                type: "text",
                nullable: true);
        }
    }
}
