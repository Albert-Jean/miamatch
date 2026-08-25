using FluentAssertions;
using Recipe.Domain.Entities;

namespace Recipes.Domain.Tests.Entities;

public class DeckTests
{
    [Fact]
    public void Create_WithValidData_ReturnsDeckWithExpectedValues()
    {
        var householdId = Guid.NewGuid();
        var recipeIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var before = DateTime.UtcNow;

        var deck = Deck.Create(householdId, recipeIds);

        var after = DateTime.UtcNow;

        deck.Id.Should().NotBeEmpty();
        deck.HouseholdId.Should().Be(householdId);
        deck.RecipeIds.Should().BeEquivalentTo(recipeIds);
        deck.GeneratedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void Create_WithNoRecipeIds_ReturnsDeckWithEmptyRecipeIds()
    {
        var deck = Deck.Create(Guid.NewGuid(), Array.Empty<Guid>());

        deck.RecipeIds.Should().BeEmpty();
    }

    [Fact]
    public void IsExpired_WhenLessThanSevenDaysOld_ReturnsFalse()
    {
        var deck = Deck.Create(Guid.NewGuid(), Array.Empty<Guid>());

        deck.IsExpired(deck.GeneratedAt.AddDays(6)).Should().BeFalse();
    }

    [Fact]
    public void IsExpired_AtExactlySevenDays_ReturnsFalse()
    {
        var deck = Deck.Create(Guid.NewGuid(), Array.Empty<Guid>());

        deck.IsExpired(deck.GeneratedAt.AddDays(7)).Should().BeFalse();
    }

    [Fact]
    public void IsExpired_WhenMoreThanSevenDaysOld_ReturnsTrue()
    {
        var deck = Deck.Create(Guid.NewGuid(), Array.Empty<Guid>());

        deck.IsExpired(deck.GeneratedAt.AddDays(8)).Should().BeTrue();
    }
}
