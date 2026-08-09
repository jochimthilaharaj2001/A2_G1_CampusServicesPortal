using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CampusServicePortal.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSampleData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "StudentMasterList",
                keyColumn: "MasterListId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "StudentMasterList",
                keyColumn: "MasterListId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "StudentMasterList",
                keyColumn: "MasterListId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "StudentMasterList",
                keyColumn: "MasterListId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "StudentMasterList",
                keyColumn: "MasterListId",
                keyValue: 5);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "StudentMasterList",
                columns: new[] { "MasterListId", "CreatedAt", "DegreeProgram", "EnrollmentYear", "Faculty", "FullName", "IndexNumber", "IsRegistered" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "BSc Engineering", 2021, "Faculty of Engineering", "Ashan Perera", "EG/2021/001", false },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "BSc Computer Science", 2021, "Faculty of Science", "Dilani Silva", "SC/2021/045", false },
                    { 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "BA Economics", 2022, "Faculty of Arts", "Kasun Fernando", "AR/2022/012", false },
                    { 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "MBBS", 2022, "Faculty of Medicine", "Nethmi Jayasinghe", "MD/2022/008", false },
                    { 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "BSc Electrical Engineering", 2023, "Faculty of Engineering", "Sahan Wickramasinghe", "EG/2023/099", false }
                });
        }
    }
}
