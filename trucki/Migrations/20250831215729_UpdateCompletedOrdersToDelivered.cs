using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace trucki.Migrations.TruckiDB
{
    public partial class UpdateCompletedOrdersToDelivered : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update all orders with Status = 8 (Completed) to Status = 7 (Delivered)
            // This is needed because we removed the Completed status from the enum
            migrationBuilder.Sql(@"
                UPDATE ""CargoOrders"" 
                SET ""Status"" = 7 
                WHERE ""Status"" = 8;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse migration: Update Status = 7 (Delivered) back to Status = 8 (Completed)
            // Note: This will update ALL delivered orders to completed, which may not be desired
            // Consider adding additional criteria if needed
            migrationBuilder.Sql(@"
                UPDATE ""CargoOrders"" 
                SET ""Status"" = 8 
                WHERE ""Status"" = 7;
            ");
        }
    }
}
