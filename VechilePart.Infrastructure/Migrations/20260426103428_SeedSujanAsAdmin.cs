using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VechilePart.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedSujanAsAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "FullName", "Phone", "Role" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), "sujanpaudel368@gmail.com", "Sujan Paudel", "+977-9800000000", 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));
        }
    }
}
