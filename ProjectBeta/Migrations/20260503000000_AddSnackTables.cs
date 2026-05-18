using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectBeta.Migrations
{
    /// <inheritdoc />
    public partial class AddSnackTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Snacks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Price = table.Column<decimal>(type: "TEXT", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: true),
                    CinemaId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Snacks", x => x.Id);
                });
                migrationBuilder.Sql(@"
                    INSERT INTO Snacks (Id, Name, Price, Quantity, CinemaId) VALUES
                    (1, 'Pepsi', 3.99, 300, 1),
                    (2, 'Cola', 3.99, 300, 1),
                    (3, 'Sprite', 3.99, 300, 1),
                    (4, 'Cola Zero', 3.99, 300, 1),
                    (5, 'Pepsi Zero', 3.99, 300, 1),
                    (6, 'Fuze Tea Peach', 3.99, 300, 1),
                    (7, 'Fuze Tea Lemon', 3.99, 300, 1),
                    (8, 'Fuze Tea Peach Sparkling', 3.99, 300, 1),
                    (9, 'Fuze Tea Lemon Sparkling', 3.99, 300, 1),
                    (10, 'Fanta Orange', 3.99, 300, 1),
                    (11, 'Spa Red', 1.99, 300, 1),
                    (12, 'Spa Blue', 1.99, 300, 1),
                    (13, 'Lipton Ice Tea Peach', 3.99, 300, 1),
                    (14, 'Lipton Ice Tea Lemon', 3.99, 300, 1),
                    (15, 'Popcorn', 6.99, 300, 1),
                    (16, 'Nachos', 7.99, 300, 1)
                ");
            migrationBuilder.CreateTable(
                name: "BookingSnacks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SnackId = table.Column<int>(type: "INTEGER", nullable: false),
                    BookingId = table.Column<int>(type: "INTEGER", nullable: false),
                    BookedQuantity = table.Column<int>(type: "INTEGER", nullable: false),
                    BookedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingSnacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingSnacks_Snacks_SnackId",
                        column: x => x.SnackId,
                        principalTable: "Snacks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BookingSnacks_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookingSnacks_SnackId",
                table: "BookingSnacks",
                column: "SnackId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingSnacks_BookingId",
                table: "BookingSnacks",
                column: "BookingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "BookingSnacks");
            migrationBuilder.DropTable(name: "Snacks");
        }
    }
}
