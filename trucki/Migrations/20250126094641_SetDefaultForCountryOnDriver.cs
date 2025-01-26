using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace trucki.Migrations.TruckiDB
{
    public partial class SetDefaultForCountryOnDriver : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DriverDocument_DocumentTypes_DocumentTypeId1",
                table: "DriverDocument");

            migrationBuilder.DropForeignKey(
                name: "FK_DriverDocument_Drivers_DriverId",
                table: "DriverDocument");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DriverDocument",
                table: "DriverDocument");

            migrationBuilder.DropIndex(
                name: "IX_DriverDocument_DocumentTypeId1",
                table: "DriverDocument");

            migrationBuilder.DropColumn(
                name: "DocumentTypeId1",
                table: "DriverDocument");

            migrationBuilder.RenameTable(
                name: "DriverDocument",
                newName: "DriverDocuments");

            migrationBuilder.RenameIndex(
                name: "IX_DriverDocument_DriverId",
                table: "DriverDocuments",
                newName: "IX_DriverDocuments_DriverId");

            migrationBuilder.AlterColumn<string>(
                name: "DocumentTypeId",
                table: "DriverDocuments",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DriverDocuments",
                table: "DriverDocuments",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_DriverDocuments_DocumentTypeId",
                table: "DriverDocuments",
                column: "DocumentTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_DriverDocuments_DocumentTypes_DocumentTypeId",
                table: "DriverDocuments",
                column: "DocumentTypeId",
                principalTable: "DocumentTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DriverDocuments_Drivers_DriverId",
                table: "DriverDocuments",
                column: "DriverId",
                principalTable: "Drivers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DriverDocuments_DocumentTypes_DocumentTypeId",
                table: "DriverDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_DriverDocuments_Drivers_DriverId",
                table: "DriverDocuments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DriverDocuments",
                table: "DriverDocuments");

            migrationBuilder.DropIndex(
                name: "IX_DriverDocuments_DocumentTypeId",
                table: "DriverDocuments");

            migrationBuilder.RenameTable(
                name: "DriverDocuments",
                newName: "DriverDocument");

            migrationBuilder.RenameIndex(
                name: "IX_DriverDocuments_DriverId",
                table: "DriverDocument",
                newName: "IX_DriverDocument_DriverId");

            migrationBuilder.AlterColumn<int>(
                name: "DocumentTypeId",
                table: "DriverDocument",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "DocumentTypeId1",
                table: "DriverDocument",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DriverDocument",
                table: "DriverDocument",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_DriverDocument_DocumentTypeId1",
                table: "DriverDocument",
                column: "DocumentTypeId1");

            migrationBuilder.AddForeignKey(
                name: "FK_DriverDocument_DocumentTypes_DocumentTypeId1",
                table: "DriverDocument",
                column: "DocumentTypeId1",
                principalTable: "DocumentTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DriverDocument_Drivers_DriverId",
                table: "DriverDocument",
                column: "DriverId",
                principalTable: "Drivers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
