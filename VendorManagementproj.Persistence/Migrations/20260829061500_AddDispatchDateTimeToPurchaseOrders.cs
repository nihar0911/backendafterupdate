using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using VendorManagementprojPersistence.Data;

#nullable disable

namespace VendorManagementprojPersistence.Migrations
{
    [DbContext(typeof(VendorManagementDbContext))]
    [Migration("20260829061500_AddDispatchDateTimeToPurchaseOrders")]
    public partial class AddDispatchDateTimeToPurchaseOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DispatchDateTime",
                table: "Purchase_Orders",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DispatchDateTime",
                table: "Purchase_Orders");
        }
    }
}
