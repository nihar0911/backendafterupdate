using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using VendorManagementprojPersistence.Data;

#nullable disable

namespace VendorManagementprojPersistence.Migrations
{
    [DbContext(typeof(VendorManagementDbContext))]
    [Migration("20260906143000_AddPurchaseOrderApproverConfiguration")]
    public partial class AddPurchaseOrderApproverConfiguration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PurchaseOrderApproverRole",
                table: "Organizations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Organization Manager");

            migrationBuilder.AddColumn<string>(
                name: "ApproverRole",
                table: "Purchase_Orders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE Purchase_Orders
                SET ApproverRole = 'Organization Manager'
                WHERE ApproverRole IS NULL
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PurchaseOrderApproverRole",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "ApproverRole",
                table: "Purchase_Orders");
        }
    }
}
