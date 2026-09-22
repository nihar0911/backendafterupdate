using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VendorManagementprojPersistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVendorFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Vendor_Feedback",
                columns: table => new
                {
                    FeedbackID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VendorID = table.Column<int>(type: "int", nullable: false),
                    OutletID = table.Column<int>(type: "int", nullable: false),
                    PurchaseOrderID = table.Column<int>(type: "int", nullable: false),
                    POItemID = table.Column<int>(type: "int", nullable: false),
                    RatedByUserID = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    ProductQualityRating = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    DeliveryRating = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    Review = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    FeedbackDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendor_Feedback", x => x.FeedbackID);
                    table.ForeignKey(
                        name: "FK_Vendor_Feedback_Outlets_OutletID",
                        column: x => x.OutletID,
                        principalTable: "Outlets",
                        principalColumn: "OutletID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vendor_Feedback_Purchase_Order_Items_POItemID",
                        column: x => x.POItemID,
                        principalTable: "Purchase_Order_Items",
                        principalColumn: "POItemID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vendor_Feedback_Purchase_Orders_PurchaseOrderID",
                        column: x => x.PurchaseOrderID,
                        principalTable: "Purchase_Orders",
                        principalColumn: "PurchaseOrderID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vendor_Feedback_Users_RatedByUserID",
                        column: x => x.RatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vendor_Feedback_Vendors_VendorID",
                        column: x => x.VendorID,
                        principalTable: "Vendors",
                        principalColumn: "VendorID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vendor_Feedback_OutletID",
                table: "Vendor_Feedback",
                column: "OutletID");

            migrationBuilder.CreateIndex(
                name: "IX_Vendor_Feedback_POItemID",
                table: "Vendor_Feedback",
                column: "POItemID");

            migrationBuilder.CreateIndex(
                name: "IX_Vendor_Feedback_PurchaseOrderID_POItemID",
                table: "Vendor_Feedback",
                columns: new[] { "PurchaseOrderID", "POItemID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vendor_Feedback_RatedByUserID",
                table: "Vendor_Feedback",
                column: "RatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Vendor_Feedback_VendorID",
                table: "Vendor_Feedback",
                column: "VendorID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Vendor_Feedback");
        }
    }
}
