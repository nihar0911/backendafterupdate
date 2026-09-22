using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VendorManagementprojPersistence.Migrations;

public partial class AddUsersAndRoles : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Roles",
            columns: table => new
            {
                RoleID = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                RoleName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Roles", x => x.RoleID);
            });

        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                UserID = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: false),
                Email = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: false),
                PasswordHash = table.Column<string>(type: "varchar(max)", unicode: false, nullable: false),
                OrganizationID = table.Column<int>(type: "int", nullable: true),
                OutletID = table.Column<int>(type: "int", nullable: true),
                RoleID = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.UserID);

                table.ForeignKey(
                    name: "FK_Users_Organizations_OrganizationID",
                    column: x => x.OrganizationID,
                    principalTable: "Organizations",
                    principalColumn: "OrganizationID",
                    onDelete: ReferentialAction.Restrict);

                table.ForeignKey(
                    name: "FK_Users_Outlets_OutletID",
                    column: x => x.OutletID,
                    principalTable: "Outlets",
                    principalColumn: "OutletID",
                    onDelete: ReferentialAction.Restrict);

                table.ForeignKey(
                    name: "FK_Users_Roles_RoleID",
                    column: x => x.RoleID,
                    principalTable: "Roles",
                    principalColumn: "RoleID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Roles_RoleName",
            table: "Roles",
            column: "RoleName",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Users_Email",
            table: "Users",
            column: "Email",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Users_OrganizationID",
            table: "Users",
            column: "OrganizationID");

        migrationBuilder.CreateIndex(
            name: "IX_Users_OutletID",
            table: "Users",
            column: "OutletID");

        migrationBuilder.CreateIndex(
            name: "IX_Users_RoleID",
            table: "Users",
            column: "RoleID");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Users");

        migrationBuilder.DropTable(
            name: "Roles");
    }
}