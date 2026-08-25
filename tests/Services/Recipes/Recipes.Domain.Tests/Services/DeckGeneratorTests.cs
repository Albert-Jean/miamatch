using FluentAssertions;
using Recipes.Domain.Services;

namespace Recipes.Domain.Tests.Services;

public class DeckGeneratorTests
{
    [Fact]
    public void GenerateRecipeIds_WhenEnoughRemainAfterExclusion_ExcludesExcludedIds()
    {
        var excluded = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var remaining = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var available = excluded.Concat(remaining).ToList();

        var result = DeckGenerator.GenerateRecipeIds(available, excluded, 2);

        result.Should().HaveCount(2);
        result.Should().NotContain(excluded);
        result.Should().OnlyContain(id => remaining.Contains(id));
    }

    [Fact]
    public void GenerateRecipeIds_WhenNotEnoughRemainAfterExclusion_FallsBackToAllAvailable()
    {
        var available = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var excluded = available;

        var result = DeckGenerator.GenerateRecipeIds(available, excluded, 2);

        result.Should().HaveCount(2);
        result.Should().OnlyContain(id => available.Contains(id));
    }

    [Fact]
    public void GenerateRecipeIds_WithFewerAvailableThanRequestedCount_ReturnsAllAvailable()
    {
        var available = new[] { Guid.NewGuid(), Guid.NewGuid() };

        var result = DeckGenerator.GenerateRecipeIds(available, Array.Empty<Guid>(), 5);

        result.Should().BeEquivalentTo(available);
    }

    [Fact]
    public void GenerateRecipeIds_ReturnsDistinctIds()
    {
        var available = Enumerable.Range(0, 10).Select(_ => Guid.NewGuid()).ToList();

        var result = DeckGenerator.GenerateRecipeIds(available, Array.Empty<Guid>(), 5);

        result.Should().OnlyHaveUniqueItems();
    }
}
