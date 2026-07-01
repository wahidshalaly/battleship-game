using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BattleshipGame.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerIdentitySubject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "identity_subject",
                table: "players",
                type: "text",
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.CreateIndex(
                name: "IX_players_identity_subject",
                table: "players",
                column: "identity_subject",
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_players_identity_subject", table: "players");

            migrationBuilder.DropColumn(name: "identity_subject", table: "players");
        }
    }
}
