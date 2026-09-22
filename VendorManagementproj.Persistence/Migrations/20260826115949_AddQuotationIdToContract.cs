using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VendorManagementprojPersistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQuotationIdToContract : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QuotationID",
                table: "Contracts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_QuotationID",
                table: "Contracts",
                column: "QuotationID");

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Quotations_QuotationID",
                table: "Contracts",
                column: "QuotationID",
                principalTable: "Quotations",
                principalColumn: "QuotationID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Quotations_QuotationID",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_QuotationID",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "QuotationID",
                table: "Contracts");
        }
    }
}
