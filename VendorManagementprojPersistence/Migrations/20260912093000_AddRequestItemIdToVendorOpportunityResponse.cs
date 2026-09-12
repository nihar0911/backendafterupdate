using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VendorManagementprojPersistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestItemIdToVendorOpportunityResponse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RequestItemID",
                table: "VendorOpportunityResponses",
                type: "int",
                nullable: true);

            // Safe Backfill: Only backfill historical records where RequestID + ProductID maps to exactly ONE PurchaseRequestItem
            migrationBuilder.Sql(@"
                UPDATE vor
                SET vor.RequestItemID = pri.RequestItemID
                FROM [VendorOpportunityResponses] vor
                INNER JOIN [Purchase_Request_Items] pri 
                    ON pri.RequestID = vor.RequestID AND pri.ProductID = vor.ProductID
                WHERE vor.RequestItemID IS NULL
                  AND (
                      SELECT COUNT(*) 
                      FROM [Purchase_Request_Items] p2 
                      WHERE p2.RequestID = vor.RequestID AND p2.ProductID = vor.ProductID
                  ) = 1;
            ");

            migrationBuilder.DropIndex(
                name: "IX_VendorOpportunityResponses_RequestID_VendorID_ProductID",
                table: "VendorOpportunityResponses");

            migrationBuilder.CreateIndex(
                name: "IX_VendorOpportunityResponses_RequestItemID_VendorID",
                table: "VendorOpportunityResponses",
                columns: new[] { "RequestItemID", "VendorID" },
                unique: true,
                filter: "[RequestItemID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_VendorOpportunityResponses_RequestID_VendorID_ProductID",
                table: "VendorOpportunityResponses",
                columns: new[] { "RequestID", "VendorID", "ProductID" });

            migrationBuilder.AddForeignKey(
                name: "FK_VendorOpportunityResponses_Purchase_Request_Items_RequestItemID",
                table: "VendorOpportunityResponses",
                column: "RequestItemID",
                principalTable: "Purchase_Request_Items",
                principalColumn: "RequestItemID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VendorOpportunityResponses_Purchase_Request_Items_RequestItemID",
                table: "VendorOpportunityResponses");

            migrationBuilder.DropIndex(
                name: "IX_VendorOpportunityResponses_RequestItemID_VendorID",
                table: "VendorOpportunityResponses");

            migrationBuilder.DropIndex(
                name: "IX_VendorOpportunityResponses_RequestID_VendorID_ProductID",
                table: "VendorOpportunityResponses");

            migrationBuilder.CreateIndex(
                name: "IX_VendorOpportunityResponses_RequestID_VendorID_ProductID",
                table: "VendorOpportunityResponses",
                columns: new[] { "RequestID", "VendorID", "ProductID" },
                unique: true);

            migrationBuilder.DropColumn(
                name: "RequestItemID",
                table: "VendorOpportunityResponses");
        }
    }
}
