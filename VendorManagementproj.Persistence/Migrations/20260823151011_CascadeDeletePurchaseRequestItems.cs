using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VendorManagementprojPersistence.Migrations
{
    /// <inheritdoc />
    public partial class CascadeDeletePurchaseRequestItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Purchase_Request_Items_Purchase_Requests_RequestID",
                table: "Purchase_Request_Items");

            migrationBuilder.AddForeignKey(
                name: "FK_Purchase_Request_Items_Purchase_Requests_RequestID",
                table: "Purchase_Request_Items",
                column: "RequestID",
                principalTable: "Purchase_Requests",
                principalColumn: "RequestID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Purchase_Request_Items_Purchase_Requests_RequestID",
                table: "Purchase_Request_Items");

            migrationBuilder.AddForeignKey(
                name: "FK_Purchase_Request_Items_Purchase_Requests_RequestID",
                table: "Purchase_Request_Items",
                column: "RequestID",
                principalTable: "Purchase_Requests",
                principalColumn: "RequestID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
