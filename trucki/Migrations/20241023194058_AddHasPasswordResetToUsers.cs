using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace trucki.Migrations.TruckiDB
{
    public partial class AddHasPasswordResetToUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasChangedPassword",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasChangedPassword",
                table: "AspNetUsers");
        }
    }
}
