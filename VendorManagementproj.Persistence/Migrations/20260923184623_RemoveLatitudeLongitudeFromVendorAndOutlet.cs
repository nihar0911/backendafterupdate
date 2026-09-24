using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VendorManagementproj.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLatitudeLongitudeFromVendorAndOutlet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Outlets");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Outlets");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "Vendors",
                type: "decimal(9,6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "Vendors",
                type: "decimal(9,6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "Outlets",
                type: "decimal(9,6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "Outlets",
                type: "decimal(9,6)",
                nullable: true);
        }
    }
}
