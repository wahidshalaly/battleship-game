using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BattleshipGame.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DeriveActiveGameFromGames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "player_game_history");

            migrationBuilder.DropIndex(name: "IX_players_active_game_id", table: "players");

            migrationBuilder.DropColumn(name: "active_game_id", table: "players");

            migrationBuilder.CreateIndex(
                name: "IX_games_player_id_active",
                table: "games",
                column: "player_id",
                unique: true,
                filter: "state <> 4"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_games_player_id_active", table: "games");

            migrationBuilder.AddColumn<Guid>(
                name: "active_game_id",
                table: "players",
                type: "uuid",
                nullable: true
            );

            migrationBuilder.CreateTable(
                name: "player_game_history",
                columns: table => new
                {
                    player_id = table.Column<Guid>(type: "uuid", nullable: false),
                    game_id = table.Column<Guid>(type: "uuid", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_game_history", x => new { x.player_id, x.game_id });
                    table.ForeignKey(
                        name: "FK_player_game_history_players_player_id",
                        column: x => x.player_id,
                        principalTable: "players",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_players_active_game_id",
                table: "players",
                column: "active_game_id",
                unique: true,
                filter: "active_game_id IS NOT NULL"
            );
        }
    }
}
