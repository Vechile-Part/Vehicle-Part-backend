using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehiclePart.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDeletedToParts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Parts",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Parts");
        }
    }
}
