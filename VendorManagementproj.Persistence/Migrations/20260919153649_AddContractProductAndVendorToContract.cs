using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VendorManagementproj.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddContractProductAndVendorToContract : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VendorID",
                table: "Contracts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Contract_Products",
                columns: table => new
                {
                    ContractProductID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractID = table.Column<int>(type: "int", nullable: false),
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    ContractQuantity = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    PurchasedQuantity = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contract_Products", x => x.ContractProductID);
                    table.ForeignKey(
                        name: "FK_Contract_Products_Contracts_ContractID",
                        column: x => x.ContractID,
                        principalTable: "Contracts",
                        principalColumn: "ContractID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Contract_Products_Products_ProductID",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_VendorID",
                table: "Contracts",
                column: "VendorID");

            migrationBuilder.CreateIndex(
                name: "IX_Contract_Products_ContractID_ProductID",
                table: "Contract_Products",
                columns: new[] { "ContractID", "ProductID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contract_Products_ProductID",
                table: "Contract_Products",
                column: "ProductID");

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Vendors_VendorID",
                table: "Contracts",
                column: "VendorID",
                principalTable: "Vendors",
                principalColumn: "VendorID",
                onDelete: ReferentialAction.Restrict);

            // Existing Data Preservation & Backfill Strategy:
            // 1. Backfill Contracts.VendorID from existing Contract_Vendor_Allocations
            migrationBuilder.Sql(@"
                UPDATE c
                SET c.VendorID = a.VendorID
                FROM Contracts c
                CROSS APPLY (
                    SELECT TOP 1 cva.VendorID 
                    FROM Contract_Vendor_Allocations cva 
                    WHERE cva.ContractID = c.ContractID 
                    ORDER BY cva.AllocationPercentage DESC
                ) a
                WHERE c.VendorID IS NULL;
            ");

            // 2. Backfill Contract_Products from existing Contracts records
            migrationBuilder.Sql(@"
                INSERT INTO Contract_Products (ContractID, ProductID, ContractQuantity, PurchasedQuantity, UnitPrice)
                SELECT c.ContractID, c.ProductID, c.TotalQuantity, c.UsedQuantity, NULL
                FROM Contracts c
                WHERE c.ProductID > 0
                  AND NOT EXISTS (
                      SELECT 1 FROM Contract_Products cp 
                      WHERE cp.ContractID = c.ContractID AND cp.ProductID = c.ProductID
                  );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Vendors_VendorID",
                table: "Contracts");

            migrationBuilder.DropTable(
                name: "Contract_Products");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_VendorID",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "VendorID",
                table: "Contracts");
        }
    }
}
