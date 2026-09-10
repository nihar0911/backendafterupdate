using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VendorManagementprojPersistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSpoilageAdviceSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SpoilageAdviceSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecentDeliveriesCount = table.Column<int>(type: "int", nullable: false),
                    TrendTolerancePercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    HighWeightedSpoilageThreshold = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    HighRecentSpoilageThreshold = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    HighMaximumSpoilageThreshold = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    MediumWeightedSpoilageThreshold = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    LowWeightedSpoilageThreshold = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpoilageAdviceSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SpoilageAdviceSettings");
        }
    }
}
