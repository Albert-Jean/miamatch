using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShoppingList.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "shoppinglist");

            migrationBuilder.CreateTable(
                name: "shopping_list_items",
                schema: "shoppinglist",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    household_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recipe_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ingredient_name = table.Column<string>(type: "text", nullable: false),
                    measure = table.Column<string>(type: "text", nullable: false),
                    added_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shopping_list_items", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "shopping_list_items",
                schema: "shoppinglist");
        }
    }
}
