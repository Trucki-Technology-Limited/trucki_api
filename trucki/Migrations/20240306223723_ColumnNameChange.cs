using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace trucki.Migrations
{
    public partial class ColumnNameChange : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Managers");

            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "Managers",
                newName: "PhoneNumber");

            migrationBuilder.AddColumn<string>(
                name: "ManagerName",
                table: "Managers",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ManagerName",
                table: "Managers");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "Managers",
                newName: "Phone");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Managers",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
