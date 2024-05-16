using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace trucki.Migrations
{
    public partial class UpdateOrderTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Businesses_Managers_ManagerId",
                table: "Businesses");

            migrationBuilder.RenameColumn(
                name: "ManagerId",
                table: "Businesses",
                newName: "managerId");

            migrationBuilder.RenameIndex(
                name: "IX_Businesses_ManagerId",
                table: "Businesses",
                newName: "IX_Businesses_managerId");

            migrationBuilder.AlterColumn<string>(
                name: "Gtv",
                table: "RoutesEnumerable",
                type: "text",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AddColumn<string>(
                name: "BusinessId",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Orders",
                type: "numeric",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_BusinessId",
                table: "Orders",
                column: "BusinessId");

            migrationBuilder.AddForeignKey(
                name: "FK_Businesses_Managers_managerId",
                table: "Businesses",
                column: "managerId",
                principalTable: "Managers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Businesses_BusinessId",
                table: "Orders",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Businesses_Managers_managerId",
                table: "Businesses");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Businesses_BusinessId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_BusinessId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "managerId",
                table: "Businesses",
                newName: "ManagerId");

            migrationBuilder.RenameIndex(
                name: "IX_Businesses_managerId",
                table: "Businesses",
                newName: "IX_Businesses_ManagerId");

            migrationBuilder.AlterColumn<float>(
                name: "Gtv",
                table: "RoutesEnumerable",
                type: "real",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddForeignKey(
                name: "FK_Businesses_Managers_ManagerId",
                table: "Businesses",
                column: "ManagerId",
                principalTable: "Managers",
                principalColumn: "Id");
        }
    }
}
