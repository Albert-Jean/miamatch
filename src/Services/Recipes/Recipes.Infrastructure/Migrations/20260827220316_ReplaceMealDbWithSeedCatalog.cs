using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recipes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceMealDbWithSeedCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The recipes table only held TheMealDB cache entries (English, untagged); the seed
            // catalog replaces them, so wipe the cache and the decks that reference it.
            migrationBuilder.Sql("DELETE FROM recipes.decks;");
            migrationBuilder.Sql("DELETE FROM recipes.recipes;");

            migrationBuilder.RenameColumn(
                name: "meal_db_id",
                schema: "recipes",
                table: "recipes",
                newName: "external_id");

            migrationBuilder.RenameIndex(
                name: "IX_recipes_meal_db_id",
                schema: "recipes",
                table: "recipes",
                newName: "IX_recipes_external_id");

            migrationBuilder.AddColumn<string[]>(
                name: "tags",
                schema: "recipes",
                table: "recipes",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "tags",
                schema: "recipes",
                table: "recipes");

            migrationBuilder.RenameColumn(
                name: "external_id",
                schema: "recipes",
                table: "recipes",
                newName: "meal_db_id");

            migrationBuilder.RenameIndex(
                name: "IX_recipes_external_id",
                schema: "recipes",
                table: "recipes",
                newName: "IX_recipes_meal_db_id");
        }
    }
}
