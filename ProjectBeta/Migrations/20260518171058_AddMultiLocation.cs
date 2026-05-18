using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectBeta.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Auditoriums_Cinemas_CinemaId",
                table: "Auditoriums");

            migrationBuilder.DropForeignKey(
                name: "FK_Auditoriums_Locations_LocationId",
                table: "Auditoriums");

            migrationBuilder.DropTable(
                name: "Cinemas");

            migrationBuilder.DropIndex(
                name: "IX_Auditoriums_CinemaId",
                table: "Auditoriums");

            migrationBuilder.DropColumn(
                name: "Capacity",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "CinemaId",
                table: "Auditoriums");

            migrationBuilder.RenameColumn(
                name: "CinemaId",
                table: "Snacks",
                newName: "LocationId");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Locations",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Locations",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "LocationId",
                table: "Auditoriums",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Auditoriums",
                keyColumn: "Id",
                keyValue: 1,
                column: "LocationId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Auditoriums",
                keyColumn: "Id",
                keyValue: 2,
                column: "LocationId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Auditoriums",
                keyColumn: "Id",
                keyValue: 3,
                column: "LocationId",
                value: 1);

            migrationBuilder.InsertData(
                table: "Locations",
                columns: new[] { "Id", "Address", "City", "Name" },
                values: new object[] { 1, "Main St 1", "Rotterdam", "Main Location" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 18, 17, 10, 57, 677, DateTimeKind.Utc).AddTicks(7516));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 18, 17, 10, 57, 677, DateTimeKind.Utc).AddTicks(7520));

            migrationBuilder.AddForeignKey(
                name: "FK_Auditoriums_Locations_LocationId",
                table: "Auditoriums",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Auditoriums_Locations_LocationId",
                table: "Auditoriums");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Locations");

            migrationBuilder.RenameColumn(
                name: "LocationId",
                table: "Snacks",
                newName: "CinemaId");

            migrationBuilder.AddColumn<int>(
                name: "Capacity",
                table: "Locations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "LocationId",
                table: "Auditoriums",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "CinemaId",
                table: "Auditoriums",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Cinemas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cinemas", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Auditoriums",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CinemaId", "LocationId" },
                values: new object[] { 1, null });

            migrationBuilder.UpdateData(
                table: "Auditoriums",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CinemaId", "LocationId" },
                values: new object[] { 1, null });

            migrationBuilder.UpdateData(
                table: "Auditoriums",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CinemaId", "LocationId" },
                values: new object[] { 1, null });

            migrationBuilder.InsertData(
                table: "Cinemas",
                columns: new[] { "Id", "City", "Name" },
                values: new object[] { 1, "Rotterdam", "Darcy" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 17, 19, 35, 13, 117, DateTimeKind.Utc).AddTicks(1740));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 17, 19, 35, 13, 117, DateTimeKind.Utc).AddTicks(1750));

            migrationBuilder.CreateIndex(
                name: "IX_Auditoriums_CinemaId",
                table: "Auditoriums",
                column: "CinemaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Auditoriums_Cinemas_CinemaId",
                table: "Auditoriums",
                column: "CinemaId",
                principalTable: "Cinemas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Auditoriums_Locations_LocationId",
                table: "Auditoriums",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
