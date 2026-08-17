using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Matching.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "matching");

            migrationBuilder.CreateTable(
                name: "matches",
                schema: "matching",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    household_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recipe_id = table.Column<Guid>(type: "uuid", nullable: false),
                    deck_id = table.Column<Guid>(type: "uuid", nullable: false),
                    matched_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_matches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "swipes",
                schema: "matching",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    household_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recipe_id = table.Column<Guid>(type: "uuid", nullable: false),
                    deck_id = table.Column<Guid>(type: "uuid", nullable: false),
                    liked = table.Column<bool>(type: "boolean", nullable: false),
                    swiped_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_swipes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_matches_household_id_recipe_id_deck_id",
                schema: "matching",
                table: "matches",
                columns: new[] { "household_id", "recipe_id", "deck_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_swipes_user_id_household_id_recipe_id_deck_id",
                schema: "matching",
                table: "swipes",
                columns: new[] { "user_id", "household_id", "recipe_id", "deck_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "matches",
                schema: "matching");

            migrationBuilder.DropTable(
                name: "swipes",
                schema: "matching");
        }
    }
}
