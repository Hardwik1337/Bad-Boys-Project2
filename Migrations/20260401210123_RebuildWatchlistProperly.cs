using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieHub.Migrations
{
    /// <inheritdoc />
    public partial class RebuildWatchlistProperly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Watchlists_Movies_MovieId",
                table: "Watchlists");

            migrationBuilder.DropIndex(
                name: "IX_Watchlists_MovieId",
                table: "Watchlists");

            migrationBuilder.DropColumn(
                name: "MovieId",
                table: "Watchlists");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Watchlists",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "WatchlistItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WatchlistId = table.Column<int>(type: "int", nullable: false),
                    MovieId = table.Column<int>(type: "int", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WatchlistItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WatchlistItems_Movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WatchlistItems_Watchlists_WatchlistId",
                        column: x => x.WatchlistId,
                        principalTable: "Watchlists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WatchlistItems_MovieId",
                table: "WatchlistItems",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_WatchlistItems_WatchlistId_MovieId",
                table: "WatchlistItems",
                columns: new[] { "WatchlistId", "MovieId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WatchlistItems");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Watchlists");

            migrationBuilder.AddColumn<int>(
                name: "MovieId",
                table: "Watchlists",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Watchlists_MovieId",
                table: "Watchlists",
                column: "MovieId");

            migrationBuilder.AddForeignKey(
                name: "FK_Watchlists_Movies_MovieId",
                table: "Watchlists",
                column: "MovieId",
                principalTable: "Movies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
