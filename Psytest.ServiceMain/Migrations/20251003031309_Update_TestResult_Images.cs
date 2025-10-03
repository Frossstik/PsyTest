using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Psytest.ServiceMain.Migrations
{
    /// <inheritdoc />
    public partial class Update_TestResult_Images : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<byte[]>>(
                name: "Images",
                table: "TestResults",
                type: "bytea[]",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Images",
                table: "TestResults");
        }
    }
}
