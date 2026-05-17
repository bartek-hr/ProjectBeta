using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectBeta.ProjectBeta.Migrations
{
    /// <inheritdoc />
    public partial class MergeSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountId",
                table: "Bookings");

            migrationBuilder.AddColumn<bool>(
                name: "HasSubscription",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SubscriptionSeatType",
                table: "Users",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BasePrice",
                table: "Bookings",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "SeatAges",
                table: "Bookings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserSeat",
                table: "Bookings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Discounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Percentage = table.Column<decimal>(type: "TEXT", nullable: false),
                    MinGroupSize = table.Column<int>(type: "INTEGER", nullable: true),
                    MaxAge = table.Column<int>(type: "INTEGER", nullable: true),
                    MinAge = table.Column<int>(type: "INTEGER", nullable: true),
                    RequiredDayOfWeek = table.Column<int>(type: "INTEGER", nullable: true),
                    EffectiveFrom = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EffectiveUntil = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Discounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SeatPrices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Price = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeatPrices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BookingDiscounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BookingId = table.Column<int>(type: "INTEGER", nullable: false),
                    DiscountId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingDiscounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingDiscounts_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookingDiscounts_Discounts_DiscountId",
                        column: x => x.DiscountId,
                        principalTable: "Discounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "HasSubscription", "SubscriptionSeatType" },
                values: new object[] { new DateTime(2026, 5, 17, 19, 31, 20, 998, DateTimeKind.Utc).AddTicks(3710), false, null });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "HasSubscription", "SubscriptionSeatType" },
                values: new object[] { new DateTime(2026, 5, 17, 19, 31, 20, 998, DateTimeKind.Utc).AddTicks(3720), false, null });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_AuditoriumId",
                table: "Bookings",
                column: "AuditoriumId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingDiscounts_BookingId",
                table: "BookingDiscounts",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingDiscounts_DiscountId",
                table: "BookingDiscounts",
                column: "DiscountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Auditoriums_AuditoriumId",
                table: "Bookings",
                column: "AuditoriumId",
                principalTable: "Auditoriums",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Auditoriums_AuditoriumId",
                table: "Bookings");

            migrationBuilder.DropTable(
                name: "BookingDiscounts");

            migrationBuilder.DropTable(
                name: "SeatPrices");

            migrationBuilder.DropTable(
                name: "Discounts");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_AuditoriumId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "HasSubscription",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SubscriptionSeatType",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BasePrice",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "SeatAges",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "UserSeat",
                table: "Bookings");

            migrationBuilder.AddColumn<int>(
                name: "DiscountId",
                table: "Bookings",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 17, 17, 31, 49, 936, DateTimeKind.Utc).AddTicks(3267));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 17, 17, 31, 49, 936, DateTimeKind.Utc).AddTicks(3270));
        }
    }
}
