using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recipes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeckMealCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "meal_count",
                schema: "recipes",
                table: "decks",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "meal_count",
                schema: "recipes",
                table: "decks");
        }
    }
}
