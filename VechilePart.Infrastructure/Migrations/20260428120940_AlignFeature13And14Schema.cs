using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehiclePart.Infrastructure.Migrations
{
    public partial class AlignFeature13And14Schema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "PartRequests",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "AppointmentAtUtc",
                table: "Appointments",
                newName: "AppointmentDate");

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                table: "ServiceReviews",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "ServiceId",
                table: "ServiceReviews",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "PartRequests",
                type: "text",
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Appointments",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "ServiceType",
                table: "Appointments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Appointments",
                type: "text",
                nullable: false,
                defaultValue: "");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ServiceId",
                table: "ServiceReviews");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "PartRequests");

            migrationBuilder.DropColumn(
                name: "ServiceType",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Appointments");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "PartRequests",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "AppointmentDate",
                table: "Appointments",
                newName: "AppointmentAtUtc");

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                table: "ServiceReviews",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Appointments",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
