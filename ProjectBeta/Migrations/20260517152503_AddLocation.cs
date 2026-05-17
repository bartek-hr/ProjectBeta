using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectBeta.Migrations
{
    /// <inheritdoc />
    public partial class AddLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LocationId",
                table: "Auditoriums",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Capacity = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Auditoriums",
                keyColumn: "Id",
                keyValue: 1,
                column: "LocationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Auditoriums",
                keyColumn: "Id",
                keyValue: 2,
                column: "LocationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Auditoriums",
                keyColumn: "Id",
                keyValue: 3,
                column: "LocationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 17, 15, 25, 3, 413, DateTimeKind.Utc).AddTicks(9499));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 17, 15, 25, 3, 413, DateTimeKind.Utc).AddTicks(9502));

            migrationBuilder.CreateIndex(
                name: "IX_Auditoriums_LocationId",
                table: "Auditoriums",
                column: "LocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Auditoriums_Locations_LocationId",
                table: "Auditoriums",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Auditoriums_Locations_LocationId",
                table: "Auditoriums");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropIndex(
                name: "IX_Auditoriums_LocationId",
                table: "Auditoriums");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "Auditoriums");

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
        }
    }
}
