using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VendorManagementprojPersistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVendorRecommendationSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VendorRecommendationSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QualityWeight = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    DeliveryWeight = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    PriceWeight = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    ReliabilityWeight = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    ReliabilityPointsPerReview = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    NeutralScoreForNewVendors = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    BestQualityThreshold = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    FastestDeliveryThreshold = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    HighQualityRationaleThreshold = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    PrioritizeActiveContracts = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorRecommendationSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VendorRecommendationSettings");
        }
    }
}
