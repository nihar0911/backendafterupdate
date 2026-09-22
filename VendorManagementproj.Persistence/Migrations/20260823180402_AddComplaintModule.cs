using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VendorManagementprojPersistence.Migrations
{
    /// <inheritdoc />
    public partial class AddComplaintModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Complaints",
                columns: table => new
                {
                    ComplaintID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VendorID = table.Column<int>(type: "int", nullable: false),
                    OutletID = table.Column<int>(type: "int", nullable: false),
                    PurchaseOrderID = table.Column<int>(type: "int", nullable: false),
                    POItemID = table.Column<int>(type: "int", nullable: false),
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    RaisedByUserID = table.Column<int>(type: "int", nullable: false),
                    ComplaintDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ComplaintType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Complaints", x => x.ComplaintID);
                    table.ForeignKey(
                        name: "FK_Complaints_Outlets_OutletID",
                        column: x => x.OutletID,
                        principalTable: "Outlets",
                        principalColumn: "OutletID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Complaints_Products_ProductID",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Complaints_Purchase_Order_Items_POItemID",
                        column: x => x.POItemID,
                        principalTable: "Purchase_Order_Items",
                        principalColumn: "POItemID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Complaints_Purchase_Orders_PurchaseOrderID",
                        column: x => x.PurchaseOrderID,
                        principalTable: "Purchase_Orders",
                        principalColumn: "PurchaseOrderID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Complaints_Users_RaisedByUserID",
                        column: x => x.RaisedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Complaints_Vendors_VendorID",
                        column: x => x.VendorID,
                        principalTable: "Vendors",
                        principalColumn: "VendorID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_OutletID",
                table: "Complaints",
                column: "OutletID");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_POItemID",
                table: "Complaints",
                column: "POItemID");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_ProductID",
                table: "Complaints",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_PurchaseOrderID",
                table: "Complaints",
                column: "PurchaseOrderID");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_RaisedByUserID",
                table: "Complaints",
                column: "RaisedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_VendorID",
                table: "Complaints",
                column: "VendorID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Complaints");
        }
    }
}
