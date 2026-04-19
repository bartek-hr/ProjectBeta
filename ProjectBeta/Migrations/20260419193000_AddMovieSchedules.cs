using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectBeta.Migrations
{
    /// <inheritdoc />
    public partial class AddMovieSchedules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MovieSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ScheduleDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    MovieId = table.Column<string>(type: "TEXT", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovieSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovieSchedules_Movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MovieSchedules_MovieId",
                table: "MovieSchedules",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_MovieSchedules_ScheduleDate",
                table: "MovieSchedules",
                column: "ScheduleDate");

            migrationBuilder.CreateIndex(
                name: "IX_MovieSchedules_ScheduleDate_StartTime",
                table: "MovieSchedules",
                columns: new[] { "ScheduleDate", "StartTime" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovieSchedules");
        }
    }
}
