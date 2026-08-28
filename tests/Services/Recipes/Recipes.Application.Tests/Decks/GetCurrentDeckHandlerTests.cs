using FluentAssertions;
using NSubstitute;
using Recipe.Domain.Entities;
using Recipes.Application.Abstractions;
using Recipes.Application.Decks;
using RecipeEntity = Recipes.Domain.Entities.Recipe;

namespace Recipes.Application.Tests.Decks;

public class GetCurrentDeckHandlerTests
{
    private readonly IDeckRepository _deckRepository = Substitute.For<IDeckRepository>();
    private readonly IRecipeRepository _recipeRepository = Substitute.For<IRecipeRepository>();
    private readonly GetCurrentDeckHandler _handler;
    private readonly Guid _householdId = Guid.NewGuid();

    public GetCurrentDeckHandlerTests()
    {
        _handler = new GetCurrentDeckHandler(_deckRepository, _recipeRepository);
    }

    private static RecipeEntity CreateRecipe(string externalId, params string[] tags) =>
        RecipeEntity.Create(externalId, $"Recipe {externalId}", "Instructions", Array.Empty<Recipes.Domain.Entities.RecipeIngredient>(), $"https://example.com/{externalId}.jpg", tags);

    [Fact]
    public async Task ExecuteAsync_WithFreshDeck_ReturnsDeckWithRecipeSummaries()
    {
        var recipes = new[] { CreateRecipe("r1", "healthy"), CreateRecipe("r2") };
        var deck = Deck.Create(_householdId, recipes.Select(r => r.Id), 2);
        _deckRepository.GetMostRecentAsync(_householdId).Returns(Task.FromResult<Deck?>(deck));
        _recipeRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>())
            .Returns(Task.FromResult<IReadOnlyCollection<RecipeEntity>>(recipes));

        var result = await _handler.ExecuteAsync(_householdId);

        result.Should().NotBeNull();
        result!.DeckId.Should().Be(deck.Id);
        result.Recipes.Should().HaveCount(2);
        result.Recipes.First().Tags.Should().BeEquivalentTo("healthy");
    }

    [Fact]
    public async Task ExecuteAsync_WithNoDeck_ReturnsNull()
    {
        _deckRepository.GetMostRecentAsync(_householdId).Returns(Task.FromResult<Deck?>(null));

        var result = await _handler.ExecuteAsync(_householdId);

        result.Should().BeNull();
    }
}
