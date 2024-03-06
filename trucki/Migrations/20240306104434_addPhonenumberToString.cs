using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace trucki.Migrations
{
    public partial class addPhonenumberToString : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Managers",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldMaxLength: 11);

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Drivers",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldMaxLength: 11);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Phone",
                table: "Managers",
                type: "integer",
                maxLength: 11,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Phone",
                table: "Drivers",
                type: "integer",
                maxLength: 11,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
