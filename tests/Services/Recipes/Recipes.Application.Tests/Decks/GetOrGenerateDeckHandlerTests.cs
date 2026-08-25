using FluentAssertions;
using NSubstitute;
using Recipe.Domain.Entities;
using Recipes.Application.Abstractions;
using Recipes.Application.Decks;
using RecipeEntity = Recipes.Domain.Entities.Recipe;

namespace Recipes.Application.Tests.Decks;

public class GetOrGenerateDeckHandlerTests
{
    private readonly IDeckRepository _deckRepository = Substitute.For<IDeckRepository>();
    private readonly IRecipeRepository _recipeRepository = Substitute.For<IRecipeRepository>();
    private readonly IRecipeCatalogClient _recipeCatalogClient = Substitute.For<IRecipeCatalogClient>();
    private readonly GetOrGenerateDeckHandler _handler;
    private readonly Guid _householdId = Guid.NewGuid();

    public GetOrGenerateDeckHandlerTests()
    {
        _handler = new GetOrGenerateDeckHandler(_deckRepository, _recipeRepository, _recipeCatalogClient);
    }

    private static RecipeEntity CreateRecipe(string mealDbId) =>
        RecipeEntity.Create(mealDbId, $"Recipe {mealDbId}", "Instructions", Array.Empty<Recipes.Domain.Entities.RecipeIngredient>(), $"https://example.com/{mealDbId}.jpg");

    [Fact]
    public async Task ExecuteAsync_WithFreshExistingDeck_ReturnsCachedDeckWithoutRegenerating()
    {
        var recipes = new[] { CreateRecipe("r1"), CreateRecipe("r2") };
        var existingDeck = Deck.Create(_householdId, recipes.Select(r => r.Id));
        _deckRepository.GetMostRecentAsync(_householdId).Returns(Task.FromResult<Deck?>(existingDeck));
        _recipeRepository.GetByIdsAsync(Arg.Is<IEnumerable<Guid>>(ids => ids.SequenceEqual(existingDeck.RecipeIds)))
            .Returns(Task.FromResult<IReadOnlyCollection<RecipeEntity>>(recipes));

        var result = await _handler.ExecuteAsync(_householdId);

        result.DeckId.Should().Be(existingDeck.Id);
        result.Recipes.Should().HaveCount(2);
        await _deckRepository.DidNotReceive().AddAsync(Arg.Any<Deck>());
        await _recipeCatalogClient.DidNotReceive().FetchRecipesAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task ExecuteAsync_WithNoExistingDeckAndSufficientPool_GeneratesNewDeckWithoutFetchingMoreRecipes()
    {
        var pool = Enumerable.Range(0, 20).Select(i => CreateRecipe($"pool-{i}")).ToList();
        _deckRepository.GetMostRecentAsync(_householdId).Returns(Task.FromResult<Deck?>(null));
        _recipeRepository.GetAllAsync().Returns(Task.FromResult<IReadOnlyCollection<RecipeEntity>>(pool));

        var result = await _handler.ExecuteAsync(_householdId);

        result.Recipes.Should().HaveCount(20);
        await _recipeCatalogClient.DidNotReceive().FetchRecipesAsync(Arg.Any<int>());
        await _recipeRepository.DidNotReceive().AddRangeAsync(Arg.Any<IEnumerable<RecipeEntity>>());
        await _deckRepository.Received(1).AddAsync(Arg.Is<Deck>(d => d.HouseholdId == _householdId && d.RecipeIds.Count == 20));
    }

    [Fact]
    public async Task ExecuteAsync_WithInsufficientPool_FetchesNewRecipesFromCatalogAndPersistsThem()
    {
        var pool = Enumerable.Range(0, 5).Select(i => CreateRecipe($"existing-{i}")).ToList();
        var fetched = Enumerable.Range(0, 15).Select(i => CreateRecipe($"new-{i}")).ToList();
        _deckRepository.GetMostRecentAsync(_householdId).Returns(Task.FromResult<Deck?>(null));
        _recipeRepository.GetAllAsync().Returns(Task.FromResult<IReadOnlyCollection<RecipeEntity>>(pool));
        _recipeCatalogClient.FetchRecipesAsync(15).Returns(Task.FromResult<IReadOnlyCollection<RecipeEntity>>(fetched));

        var result = await _handler.ExecuteAsync(_householdId);

        await _recipeCatalogClient.Received(1).FetchRecipesAsync(15);
        await _recipeRepository.Received(1).AddRangeAsync(Arg.Is<IEnumerable<RecipeEntity>>(rs => rs.Count() == 15));
        result.Recipes.Should().HaveCount(20);
    }

    [Fact]
    public async Task ExecuteAsync_WhenFetchedRecipesAreDuplicates_KeepsFetchingUntilEnoughNewRecipes()
    {
        var pool = Enumerable.Range(0, 19).Select(i => CreateRecipe($"existing-{i}")).ToList();
        var duplicateBatch = new[] { CreateRecipe("existing-5") };
        var freshBatch = new[] { CreateRecipe("brand-new") };
        _deckRepository.GetMostRecentAsync(_householdId).Returns(Task.FromResult<Deck?>(null));
        _recipeRepository.GetAllAsync().Returns(Task.FromResult<IReadOnlyCollection<RecipeEntity>>(pool));
        _recipeCatalogClient.FetchRecipesAsync(1).Returns(
            Task.FromResult<IReadOnlyCollection<RecipeEntity>>(duplicateBatch),
            Task.FromResult<IReadOnlyCollection<RecipeEntity>>(freshBatch));

        await _handler.ExecuteAsync(_householdId);

        await _recipeCatalogClient.Received(2).FetchRecipesAsync(1);
        await _recipeRepository.Received(1).AddRangeAsync(Arg.Is<IEnumerable<RecipeEntity>>(rs =>
            rs.Count() == 1 && rs.Single().MealDbId == "brand-new"));
    }

    [Fact]
    public async Task ExecuteAsync_WithNoExistingDeckAndEmptyRepository_GeneratesDeckExcludingNothing()
    {
        var pool = Enumerable.Range(0, 20).Select(i => CreateRecipe($"pool-{i}")).ToList();
        _deckRepository.GetMostRecentAsync(_householdId).Returns(Task.FromResult<Deck?>(null));
        _recipeRepository.GetAllAsync().Returns(Task.FromResult<IReadOnlyCollection<RecipeEntity>>(pool));

        var result = await _handler.ExecuteAsync(_householdId);

        result.Recipes.Select(r => r.Id).Should().BeSubsetOf(pool.Select(r => r.Id));
    }
}
