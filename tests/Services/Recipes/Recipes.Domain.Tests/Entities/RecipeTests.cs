using FluentAssertions;
using Recipes.Domain.Entities;

namespace Recipes.Domain.Tests.Entities;

public class RecipeTests
{
    [Fact]
    public void Create_WithValidData_ReturnsRecipeWithExpectedValues()
    {
        var ingredients = new List<RecipeIngredient>
        {
            new("Tomato", "2 units"),
            new("Salt", "1 tsp"),
        };
        var before = DateTime.UtcNow;

        var recipe = Recipes.Domain.Entities.Recipe.Create("poulet-basquaise", "Poulet basquaise", "Mix everything.", ingredients, "https://example.com/image.jpg", new[] { "proteine", "healthy" });

        var after = DateTime.UtcNow;

        recipe.Id.Should().NotBeEmpty();
        recipe.ExternalId.Should().Be("poulet-basquaise");
        recipe.Name.Should().Be("Poulet basquaise");
        recipe.Instructions.Should().Be("Mix everything.");
        recipe.Ingredients.Should().BeEquivalentTo(ingredients);
        recipe.ImageUrl.Should().Be("https://example.com/image.jpg");
        recipe.Tags.Should().BeEquivalentTo("proteine", "healthy");
        recipe.CacheAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void Create_WithoutTags_ReturnsRecipeWithEmptyTags()
    {
        var recipe = Recipes.Domain.Entities.Recipe.Create("1", "A", "Instr", Array.Empty<RecipeIngredient>(), "url");

        recipe.Tags.Should().BeEmpty();
    }

    [Fact]
    public void Create_CalledTwice_GeneratesDistinctIds()
    {
        var ingredients = Array.Empty<RecipeIngredient>();

        var first = Recipes.Domain.Entities.Recipe.Create("1", "A", "Instr", ingredients, "url");
        var second = Recipes.Domain.Entities.Recipe.Create("2", "B", "Instr", ingredients, "url");

        first.Id.Should().NotBe(second.Id);
    }
}
