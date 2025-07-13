using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace trucki.Migrations.TruckiDB
{
    public partial class ConvertTruckTypeToString : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DriverRatings_CargoOrderId",
                table: "DriverRatings");

            // Step 1: Add temporary column for string conversion
            migrationBuilder.AddColumn<string>(
                name: "TruckTypeTemp",
                table: "Trucks",
                type: "text",
                nullable: false,
                defaultValue: "");

            // Step 2: Convert existing enum values to meaningful string names
            migrationBuilder.Sql(@"
                UPDATE ""Trucks"" 
                SET ""TruckTypeTemp"" = CASE ""TruckType""
                    WHEN 0 THEN 'Flatbed'
                    WHEN 1 THEN 'BoxBody'
                    WHEN 2 THEN 'Bucket Body'
                    WHEN 3 THEN 'Lowbed'
                    WHEN 4 THEN 'Containerized Body'
                    WHEN 5 THEN 'Refrigerator'
                    WHEN 6 THEN 'Cargo Van'
                    ELSE 'Cargo Van'
                END;
            ");

            // Step 3: Drop the old integer column
            migrationBuilder.DropColumn(
                name: "TruckType",
                table: "Trucks");

            // Step 4: Rename temp column to original name
            migrationBuilder.RenameColumn(
                name: "TruckTypeTemp",
                table: "Trucks",
                newName: "TruckType");

            // Step 5: Recreate the DriverRatings index as unique
            migrationBuilder.CreateIndex(
                name: "IX_DriverRatings_CargoOrderId",
                table: "DriverRatings",
                column: "CargoOrderId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DriverRatings_CargoOrderId",
                table: "DriverRatings");

            // Step 1: Add temp integer column
            migrationBuilder.AddColumn<int>(
                name: "TruckTypeTemp",
                table: "Trucks",
                type: "integer",
                nullable: false,
                defaultValue: 6); // Default to CargoVan

            // Step 2: Convert string values back to enum integers
            migrationBuilder.Sql(@"
                UPDATE ""Trucks"" 
                SET ""TruckTypeTemp"" = CASE ""TruckType""
                    WHEN 'Flatbed' THEN 0
                    WHEN 'BoxBody' THEN 1
                    WHEN 'Bucket Body' THEN 2
                    WHEN 'Low bed' THEN 3
                    WHEN 'Containerized Body' THEN 4
                    WHEN 'Refrigerator' THEN 5
                    WHEN 'Cargo Van' THEN 6
                    ELSE 6
                END;
            ");

            // Step 3: Drop string column
            migrationBuilder.DropColumn(
                name: "TruckType",
                table: "Trucks");

            // Step 4: Rename temp column back
            migrationBuilder.RenameColumn(
                name: "TruckTypeTemp",
                table: "Trucks",
                newName: "TruckType");

            // Step 5: Recreate the DriverRatings index (non-unique)
            migrationBuilder.CreateIndex(
                name: "IX_DriverRatings_CargoOrderId",
                table: "DriverRatings",
                column: "CargoOrderId");
        }
    }
}