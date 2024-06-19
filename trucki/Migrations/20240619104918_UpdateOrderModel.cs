using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace trucki.Migrations
{
    public partial class UpdateOrderModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Officers_OfficerId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_OfficerId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OfficerId",
                table: "Orders");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OfficerId",
                table: "Orders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OfficerId",
                table: "Orders",
                column: "OfficerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Officers_OfficerId",
                table: "Orders",
                column: "OfficerId",
                principalTable: "Officers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
