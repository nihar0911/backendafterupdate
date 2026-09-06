using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using VendorManagementprojPersistence.Data;

#nullable disable

namespace VendorManagementprojPersistence.Migrations
{
    [DbContext(typeof(VendorManagementDbContext))]
    [Migration("20260906144500_MovePurchaseOrderApproverToOutlet")]
    public partial class MovePurchaseOrderApproverToOutlet : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PurchaseOrderApproverRole",
                table: "Outlets",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Organization Manager");

            migrationBuilder.Sql("""
                IF COL_LENGTH('Organizations', 'PurchaseOrderApproverRole') IS NOT NULL
                BEGIN
                    UPDATE o
                    SET o.PurchaseOrderApproverRole = org.PurchaseOrderApproverRole
                    FROM Outlets o
                    INNER JOIN Organizations org ON org.OrganizationID = o.OrganizationID
                    WHERE org.PurchaseOrderApproverRole IS NOT NULL
                      AND LTRIM(RTRIM(org.PurchaseOrderApproverRole)) <> ''
                END
                """);

            migrationBuilder.DropColumn(
                name: "PurchaseOrderApproverRole",
                table: "Organizations");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PurchaseOrderApproverRole",
                table: "Organizations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Organization Manager");

            migrationBuilder.Sql("""
                UPDATE org
                SET org.PurchaseOrderApproverRole = outlet_roles.PurchaseOrderApproverRole
                FROM Organizations org
                INNER JOIN (
                    SELECT OrganizationID, MIN(PurchaseOrderApproverRole) AS PurchaseOrderApproverRole
                    FROM Outlets
                    GROUP BY OrganizationID
                ) outlet_roles ON outlet_roles.OrganizationID = org.OrganizationID
                """);

            migrationBuilder.DropColumn(
                name: "PurchaseOrderApproverRole",
                table: "Outlets");
        }
    }
}
