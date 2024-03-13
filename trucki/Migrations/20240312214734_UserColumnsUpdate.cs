using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace trucki.Migrations
{
    public partial class UserColumnsUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Managers_Company_CompanyId",
                table: "Managers");

            migrationBuilder.AlterColumn<string>(
                name: "CompanyId",
                table: "Managers",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Managers_Company_CompanyId",
                table: "Managers",
                column: "CompanyId",
                principalTable: "Company",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Managers_Company_CompanyId",
                table: "Managers");

            migrationBuilder.AlterColumn<string>(
                name: "CompanyId",
                table: "Managers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddForeignKey(
                name: "FK_Managers_Company_CompanyId",
                table: "Managers",
                column: "CompanyId",
                principalTable: "Company",
                principalColumn: "Id");
        }
    }
}
