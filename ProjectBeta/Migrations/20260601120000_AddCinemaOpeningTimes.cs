using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using ProjectBeta.Data;

#nullable disable

namespace ProjectBeta.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260601120000_AddCinemaOpeningTimes")]
    public partial class AddCinemaOpeningTimes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CinemaOpeningTimes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CinemaId = table.Column<int>(type: "INTEGER", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    ExpiresAt = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    OpeningTime = table.Column<TimeOnly>(type: "TEXT", nullable: true),
                    ClosingTime = table.Column<TimeOnly>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CinemaOpeningTimes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CinemaOpeningTimes_Cinemas_CinemaId",
                        column: x => x.CinemaId,
                        principalTable: "Cinemas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CinemaOpeningTimes_CinemaId_CreatedAt",
                table: "CinemaOpeningTimes",
                columns: new[] { "CinemaId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CinemaOpeningTimes_CinemaId_StartDate_ExpiresAt",
                table: "CinemaOpeningTimes",
                columns: new[] { "CinemaId", "StartDate", "ExpiresAt" });

            migrationBuilder.Sql("""
                INSERT INTO CinemaOpeningTimes (CinemaId, StartDate, ExpiresAt, OpeningTime, ClosingTime, CreatedAt)
                SELECT Id, '0001-01-01', '9999-12-31', '09:00:00', '20:00:00', '0001-01-01 00:00:00'
                FROM Cinemas
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CinemaOpeningTimes");
        }
    }
}
