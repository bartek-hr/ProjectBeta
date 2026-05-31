using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectBeta.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditoriumToMovieSchedules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM \"MovieSchedules\";");

            migrationBuilder.DropIndex(
                name: "IX_MovieSchedules_ScheduleDate_StartTime",
                table: "MovieSchedules");

            migrationBuilder.AddColumn<int>(
                name: "AuditoriumId",
                table: "MovieSchedules",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 11, 13, 47, 48, 290, DateTimeKind.Utc).AddTicks(4750));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 11, 13, 47, 48, 290, DateTimeKind.Utc).AddTicks(4750));

            migrationBuilder.CreateIndex(
                name: "IX_MovieSchedules_AuditoriumId",
                table: "MovieSchedules",
                column: "AuditoriumId");

            migrationBuilder.CreateIndex(
                name: "IX_MovieSchedules_ScheduleDate_AuditoriumId_StartTime",
                table: "MovieSchedules",
                columns: new[] { "ScheduleDate", "AuditoriumId", "StartTime" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MovieSchedules_Auditoriums_AuditoriumId",
                table: "MovieSchedules",
                column: "AuditoriumId",
                principalTable: "Auditoriums",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovieSchedules_Auditoriums_AuditoriumId",
                table: "MovieSchedules");

            migrationBuilder.DropIndex(
                name: "IX_MovieSchedules_AuditoriumId",
                table: "MovieSchedules");

            migrationBuilder.DropIndex(
                name: "IX_MovieSchedules_ScheduleDate_AuditoriumId_StartTime",
                table: "MovieSchedules");

            migrationBuilder.DropColumn(
                name: "AuditoriumId",
                table: "MovieSchedules");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 2, 11, 6, 12, 533, DateTimeKind.Utc).AddTicks(980));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 2, 11, 6, 12, 533, DateTimeKind.Utc).AddTicks(980));

            migrationBuilder.CreateIndex(
                name: "IX_MovieSchedules_ScheduleDate_StartTime",
                table: "MovieSchedules",
                columns: new[] { "ScheduleDate", "StartTime" },
                unique: true);
        }
    }
}
