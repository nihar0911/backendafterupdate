using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VendorManagementprojPersistence.Migrations
{
    /// <inheritdoc />
    public partial class AddContracts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Contracts",
                columns: table => new
                {
                    ContractID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OutletID = table.Column<int>(type: "int", nullable: false),
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    TotalQuantity = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    UsedQuantity = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contracts", x => x.ContractID);
                    table.ForeignKey(
                        name: "FK_Contracts_Outlets_OutletID",
                        column: x => x.OutletID,
                        principalTable: "Outlets",
                        principalColumn: "OutletID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contracts_Products_ProductID",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Contract_Vendor_Allocations",
                columns: table => new
                {
                    ContractVendorAllocationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractID = table.Column<int>(type: "int", nullable: false),
                    VendorID = table.Column<int>(type: "int", nullable: false),
                    AllocationPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    AllocatedQuantity = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    UsedQuantity = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contract_Vendor_Allocations", x => x.ContractVendorAllocationID);
                    table.ForeignKey(
                        name: "FK_Contract_Vendor_Allocations_Contracts_ContractID",
                        column: x => x.ContractID,
                        principalTable: "Contracts",
                        principalColumn: "ContractID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Contract_Vendor_Allocations_Vendors_VendorID",
                        column: x => x.VendorID,
                        principalTable: "Vendors",
                        principalColumn: "VendorID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contract_Vendor_Allocations_ContractID_VendorID",
                table: "Contract_Vendor_Allocations",
                columns: new[] { "ContractID", "VendorID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contract_Vendor_Allocations_VendorID",
                table: "Contract_Vendor_Allocations",
                column: "VendorID");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_OutletID",
                table: "Contracts",
                column: "OutletID");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_ProductID",
                table: "Contracts",
                column: "ProductID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Contract_Vendor_Allocations");

            migrationBuilder.DropTable(
                name: "Contracts");
        }
    }
}
