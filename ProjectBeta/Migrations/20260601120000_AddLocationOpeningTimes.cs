using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using ProjectBeta.Data;

#nullable disable

namespace ProjectBeta.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260601120000_AddLocationOpeningTimes")]
    public partial class AddLocationOpeningTimes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LocationOpeningTimes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LocationId = table.Column<int>(type: "INTEGER", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    ExpiresAt = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    OpeningTime = table.Column<TimeOnly>(type: "TEXT", nullable: true),
                    ClosingTime = table.Column<TimeOnly>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationOpeningTimes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocationOpeningTimes_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LocationOpeningTimes_LocationId_CreatedAt",
                table: "LocationOpeningTimes",
                columns: new[] { "LocationId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_LocationOpeningTimes_LocationId_StartDate_ExpiresAt",
                table: "LocationOpeningTimes",
                columns: new[] { "LocationId", "StartDate", "ExpiresAt" });

            migrationBuilder.Sql("""
                INSERT INTO LocationOpeningTimes (LocationId, StartDate, ExpiresAt, OpeningTime, ClosingTime, CreatedAt)
                SELECT Id, '0001-01-01', '9999-12-31', '09:00:00', '20:00:00', '0001-01-01 00:00:00'
                FROM Locations
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LocationOpeningTimes");
        }
    }
}
