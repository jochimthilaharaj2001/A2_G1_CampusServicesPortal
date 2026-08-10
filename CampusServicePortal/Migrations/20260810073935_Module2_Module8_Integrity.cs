using CampusServicePortal.Data;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace CampusServicePortal.Migrations
{
    /// <summary>
    /// Focused Module 2/8 data-integrity migration. It deliberately avoids
    /// unrelated legacy schema changes that are already represented in the
    /// deployed database.
    /// </summary>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260810073935_Module2_Module8_Integrity")]
    public partial class Module2_Module8_Integrity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Notifications",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "System");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "HostelApplications",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.DropForeignKey(
                name: "FK_HostelApplications_Hostels_HostelId",
                table: "HostelApplications");
            migrationBuilder.DropForeignKey(
                name: "FK_HostelApplications_Rooms_RoomId",
                table: "HostelApplications");
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Students_StudentId",
                table: "Notifications");
            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_Hostels_HostelId",
                table: "Rooms");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Rooms_CapacityAndOccupancy",
                table: "Rooms",
                sql: "[Capacity] > 0 AND [CurrentOccupancy] >= 0 AND [CurrentOccupancy] <= [Capacity]");

            migrationBuilder.CreateIndex(
                name: "IX_Hostels_HostelName",
                table: "Hostels",
                column: "HostelName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_HostelId_RoomNumber",
                table: "Rooms",
                columns: new[] { "HostelId", "RoomNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_StudentId_CreatedDate",
                table: "Notifications",
                columns: new[] { "StudentId", "CreatedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_HostelApplications_StudentId_Status",
                table: "HostelApplications",
                columns: new[] { "StudentId", "Status" });

            migrationBuilder.AddForeignKey(
                name: "FK_HostelApplications_Hostels_HostelId",
                table: "HostelApplications",
                column: "HostelId",
                principalTable: "Hostels",
                principalColumn: "HostelId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HostelApplications_Rooms_RoomId",
                table: "HostelApplications",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "RoomId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Students_StudentId",
                table: "Notifications",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_Hostels_HostelId",
                table: "Rooms",
                column: "HostelId",
                principalTable: "Hostels",
                principalColumn: "HostelId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HostelApplications_Hostels_HostelId",
                table: "HostelApplications");
            migrationBuilder.DropForeignKey(
                name: "FK_HostelApplications_Rooms_RoomId",
                table: "HostelApplications");
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Students_StudentId",
                table: "Notifications");
            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_Hostels_HostelId",
                table: "Rooms");

            migrationBuilder.DropIndex(name: "IX_Hostels_HostelName", table: "Hostels");
            migrationBuilder.DropIndex(name: "IX_Rooms_HostelId_RoomNumber", table: "Rooms");
            migrationBuilder.DropIndex(name: "IX_Notifications_StudentId_CreatedDate", table: "Notifications");
            migrationBuilder.DropIndex(name: "IX_HostelApplications_StudentId_Status", table: "HostelApplications");
            migrationBuilder.DropCheckConstraint(name: "CK_Rooms_CapacityAndOccupancy", table: "Rooms");

            migrationBuilder.DropColumn(name: "Type", table: "Notifications");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "HostelApplications",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddForeignKey(
                name: "FK_HostelApplications_Hostels_HostelId",
                table: "HostelApplications",
                column: "HostelId",
                principalTable: "Hostels",
                principalColumn: "HostelId",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_HostelApplications_Rooms_RoomId",
                table: "HostelApplications",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "RoomId");
            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Students_StudentId",
                table: "Notifications",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_Hostels_HostelId",
                table: "Rooms",
                column: "HostelId",
                principalTable: "Hostels",
                principalColumn: "HostelId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
