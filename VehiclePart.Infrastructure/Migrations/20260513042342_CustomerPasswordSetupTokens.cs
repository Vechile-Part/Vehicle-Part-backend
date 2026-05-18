using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehiclePart.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CustomerPasswordSetupTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomerPasswordSetupTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "text", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerPasswordSetupTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerPasswordSetupTokens_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerPasswordSetupTokens_CustomerId",
                table: "CustomerPasswordSetupTokens",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerPasswordSetupTokens_TokenHash",
                table: "CustomerPasswordSetupTokens",
                column: "TokenHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerPasswordSetupTokens");
        }
    }
}
