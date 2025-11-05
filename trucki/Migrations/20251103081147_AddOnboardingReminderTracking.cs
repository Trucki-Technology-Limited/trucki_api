using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace trucki.Migrations.TruckiDB
{
    public partial class AddOnboardingReminderTracking : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OnboardingReminderTrackings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    DriverId = table.Column<string>(type: "text", nullable: false),
                    FirstReminderSent = table.Column<bool>(type: "boolean", nullable: false),
                    FirstReminderSentAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    SecondReminderSent = table.Column<bool>(type: "boolean", nullable: false),
                    SecondReminderSentAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    OnboardingStartedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnboardingReminderTrackings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OnboardingReminderTrackings_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingReminderTrackings_DriverId",
                table: "OnboardingReminderTrackings",
                column: "DriverId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OnboardingReminderTrackings");
        }
    }
}
