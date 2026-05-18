using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehiclePart.Infrastructure.Migrations
{
    public partial class NotificationReminderStateAndJob : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastCreditReminderSentUtc",
                table: "SalesInvoices",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NotificationJobStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LastLowStockDigestSentUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationJobStates", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationJobStates");

            migrationBuilder.DropColumn(
                name: "LastCreditReminderSentUtc",
                table: "SalesInvoices");
        }
    }
}
