using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VendorManagementprojPersistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Delivery_Records",
                columns: table => new
                {
                    DeliveryRecordID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseOrderID = table.Column<int>(type: "int", nullable: false),
                    POItemID = table.Column<int>(type: "int", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrderedQuantity = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ReceivedQuantity = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    SpoiledQuantity = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    SpoilagePercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ConfirmedByUserID = table.Column<int>(type: "int", nullable: true),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Delivery_Records", x => x.DeliveryRecordID);
                    table.ForeignKey(
                        name: "FK_Delivery_Records_Purchase_Order_Items_POItemID",
                        column: x => x.POItemID,
                        principalTable: "Purchase_Order_Items",
                        principalColumn: "POItemID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Delivery_Records_Purchase_Orders_PurchaseOrderID",
                        column: x => x.PurchaseOrderID,
                        principalTable: "Purchase_Orders",
                        principalColumn: "PurchaseOrderID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Delivery_Records_Users_ConfirmedByUserID",
                        column: x => x.ConfirmedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Delivery_Records_ConfirmedByUserID",
                table: "Delivery_Records",
                column: "ConfirmedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Delivery_Records_POItemID",
                table: "Delivery_Records",
                column: "POItemID");

            migrationBuilder.CreateIndex(
                name: "IX_Delivery_Records_PurchaseOrderID",
                table: "Delivery_Records",
                column: "PurchaseOrderID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Delivery_Records");
        }
    }
}
