using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehiclePart.Infrastructure.Migrations
{
    public partial class AddVendorIdToParts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "VendorId",
                table: "Parts",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VendorId",
                table: "Parts");
        }
    }
}
