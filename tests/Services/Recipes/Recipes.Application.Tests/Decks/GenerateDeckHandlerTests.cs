using FluentAssertions;
using NSubstitute;
using Recipe.Domain.Entities;
using Recipes.Application.Abstractions;
using Recipes.Application.Decks;
using RecipeEntity = Recipes.Domain.Entities.Recipe;

namespace Recipes.Application.Tests.Decks;

public class GenerateDeckHandlerTests
{
    private readonly IDeckRepository _deckRepository = Substitute.For<IDeckRepository>();
    private readonly IRecipeRepository _recipeRepository = Substitute.For<IRecipeRepository>();
    private readonly IRecipeCatalog _recipeCatalog = Substitute.For<IRecipeCatalog>();
    private readonly GenerateDeckHandler _handler;
    private readonly Guid _householdId = Guid.NewGuid();

    public GenerateDeckHandlerTests()
    {
        _handler = new GenerateDeckHandler(_deckRepository, _recipeRepository, _recipeCatalog);
        _recipeCatalog.GetRecipesAsync().Returns(Task.FromResult<IReadOnlyCollection<RecipeEntity>>(Array.Empty<RecipeEntity>()));
    }

    private static RecipeEntity CreateRecipe(string externalId, params string[] tags) =>
        RecipeEntity.Create(externalId, $"Recipe {externalId}", "Instructions", Array.Empty<Recipes.Domain.Entities.RecipeIngredient>(), $"https://example.com/{externalId}.jpg", tags);

    [Fact]
    public async Task ExecuteAsync_WithFreshExistingDeck_ReturnsCachedDeckWithoutRegenerating()
    {
        var recipes = new[] { CreateRecipe("r1"), CreateRecipe("r2") };
        var existingDeck = Deck.Create(_householdId, recipes.Select(r => r.Id));
        _deckRepository.GetMostRecentAsync(_householdId).Returns(Task.FromResult<Deck?>(existingDeck));
        _recipeRepository.GetByIdsAsync(Arg.Is<IEnumerable<Guid>>(ids => ids.SequenceEqual(existingDeck.RecipeIds)))
            .Returns(Task.FromResult<IReadOnlyCollection<RecipeEntity>>(recipes));

        var result = await _handler.ExecuteAsync(_householdId, Array.Empty<string>());

        result.DeckId.Should().Be(existingDeck.Id);
        result.Recipes.Should().HaveCount(2);
        await _deckRepository.DidNotReceive().AddAsync(Arg.Any<Deck>());
        await _recipeCatalog.DidNotReceive().GetRecipesAsync();
    }

    [Fact]
    public async Task ExecuteAsync_WithNoExistingDeck_GeneratesDeckFromPool()
    {
        var pool = Enumerable.Range(0, 30).Select(i => CreateRecipe($"pool-{i}")).ToList();
        _deckRepository.GetMostRecentAsync(_householdId).Returns(Task.FromResult<Deck?>(null));
        _recipeRepository.GetAllAsync().Returns(Task.FromResult<IReadOnlyCollection<RecipeEntity>>(pool));

        var result = await _handler.ExecuteAsync(_householdId, Array.Empty<string>());

        result.Recipes.Should().HaveCount(20);
        await _deckRepository.Received(1).AddAsync(Arg.Is<Deck>(d => d.HouseholdId == _householdId && d.RecipeIds.Count == 20));
        await _recipeRepository.DidNotReceive().AddRangeAsync(Arg.Any<IEnumerable<RecipeEntity>>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCatalogHasRecipesMissingFromRepository_SeedsOnlyMissingOnes()
    {
        var existing = CreateRecipe("existing");
        var missing = CreateRecipe("missing");
        var duplicateOfExisting = CreateRecipe("existing");
        _deckRepository.GetMostRecentAsync(_householdId).Returns(Task.FromResult<Deck?>(null));
        _recipeRepository.GetAllAsync().Returns(Task.FromResult<IReadOnlyCollection<RecipeEntity>>(new[] { existing }));
        _recipeCatalog.GetRecipesAsync().Returns(Task.FromResult<IReadOnlyCollection<RecipeEntity>>(new[] { duplicateOfExisting, missing }));

        var result = await _handler.ExecuteAsync(_householdId, Array.Empty<string>());

        await _recipeRepository.Received(1).AddRangeAsync(Arg.Is<IEnumerable<RecipeEntity>>(rs =>
            rs.Count() == 1 && rs.Single().ExternalId == "missing"));
        result.Recipes.Should().HaveCount(2);
    }

    [Fact]
    public async Task ExecuteAsync_WithCategories_OnlySelectsRecipesMatchingAtLeastOneCategory()
    {
        var healthy = Enumerable.Range(0, 10).Select(i => CreateRecipe($"healthy-{i}", "healthy")).ToList();
        var comfort = Enumerable.Range(0, 10).Select(i => CreateRecipe($"comfort-{i}", "comfort")).ToList();
        _deckRepository.GetMostRecentAsync(_householdId).Returns(Task.FromResult<Deck?>(null));
        _recipeRepository.GetAllAsync().Returns(Task.FromResult<IReadOnlyCollection<RecipeEntity>>(healthy.Concat(comfort).ToList()));

        var result = await _handler.ExecuteAsync(_householdId, new[] { "healthy" });

        result.Recipes.Should().HaveCount(10);
        result.Recipes.Select(r => r.Id).Should().BeSubsetOf(healthy.Select(r => r.Id));
    }

    [Fact]
    public async Task ExecuteAsync_WithCategories_NormalizesCasingAndWhitespace()
    {
        var healthy = Enumerable.Range(0, 5).Select(i => CreateRecipe($"healthy-{i}", "healthy")).ToList();
        var comfort = Enumerable.Range(0, 5).Select(i => CreateRecipe($"comfort-{i}", "comfort")).ToList();
        _deckRepository.GetMostRecentAsync(_householdId).Returns(Task.FromResult<Deck?>(null));
        _recipeRepository.GetAllAsync().Returns(Task.FromResult<IReadOnlyCollection<RecipeEntity>>(healthy.Concat(comfort).ToList()));

        var result = await _handler.ExecuteAsync(_householdId, new[] { "  Healthy " });

        result.Recipes.Select(r => r.Id).Should().BeSubsetOf(healthy.Select(r => r.Id));
    }

    [Fact]
    public async Task ExecuteAsync_WithExpiredDeck_ExcludesPreviousDeckRecipes()
    {
        var pool = Enumerable.Range(0, 40).Select(i => CreateRecipe($"pool-{i}")).ToList();
        var previousIds = pool.Take(20).Select(r => r.Id).ToList();
        var expiredDeck = CreateExpiredDeck(previousIds);
        _deckRepository.GetMostRecentAsync(_householdId).Returns(Task.FromResult<Deck?>(expiredDeck));
        _recipeRepository.GetAllAsync().Returns(Task.FromResult<IReadOnlyCollection<RecipeEntity>>(pool));

        var result = await _handler.ExecuteAsync(_householdId, Array.Empty<string>());

        result.Recipes.Select(r => r.Id).Should().NotIntersectWith(previousIds);
    }

    [Fact]
    public async Task ExecuteAsync_WithFewerCandidatesThanDeckSize_GeneratesSmallerDeck()
    {
        var pool = Enumerable.Range(0, 6).Select(i => CreateRecipe($"veggie-{i}", "vegetarien")).ToList();
        _deckRepository.GetMostRecentAsync(_householdId).Returns(Task.FromResult<Deck?>(null));
        _recipeRepository.GetAllAsync().Returns(Task.FromResult<IReadOnlyCollection<RecipeEntity>>(pool));

        var result = await _handler.ExecuteAsync(_householdId, new[] { "vegetarien" });

        result.Recipes.Should().HaveCount(6);
    }

    private static Deck CreateExpiredDeck(IEnumerable<Guid> recipeIds)
    {
        var deck = Deck.Create(Guid.NewGuid(), recipeIds);
        // Deck.Create stamps GeneratedAt with UtcNow, so rewind it through the backing field to simulate an old deck.
        var field = typeof(Deck).GetProperty(nameof(Deck.GeneratedAt))!.GetBackingField();
        field.SetValue(deck, DateTime.UtcNow.AddDays(-8));
        return deck;
    }
}

internal static class ReflectionExtensions
{
    public static System.Reflection.FieldInfo GetBackingField(this System.Reflection.PropertyInfo property) =>
        property.DeclaringType!.GetField($"<{property.Name}>k__BackingField",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
}
