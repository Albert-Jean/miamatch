using FluentAssertions;
using Matching.Domain.Entities;

namespace Matching.Domain.Tests.Entities;

public class SwipeTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSwipeWithExpectedValues()
    {
        var userId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        var recipeId = Guid.NewGuid();
        var deckId = Guid.NewGuid();
        var before = DateTime.UtcNow;

        var swipe = Swipe.Create(userId, householdId, recipeId, deckId, liked: true);

        var after = DateTime.UtcNow;

        swipe.Id.Should().NotBeEmpty();
        swipe.UserId.Should().Be(userId);
        swipe.HouseholdId.Should().Be(householdId);
        swipe.RecipeId.Should().Be(recipeId);
        swipe.DeckId.Should().Be(deckId);
        swipe.Liked.Should().BeTrue();
        swipe.SwipedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void Create_CalledTwice_GeneratesDistinctIds()
    {
        var first = Swipe.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), liked: true);
        var second = Swipe.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), liked: true);

        first.Id.Should().NotBe(second.Id);
    }

    [Fact]
    public void ChangeDecision_UpdatesLikedAndSwipedAt()
    {
        var swipe = Swipe.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), liked: false);
        var originalSwipedAt = swipe.SwipedAt;

        swipe.ChangeDecision(true);

        swipe.Liked.Should().BeTrue();
        swipe.SwipedAt.Should().BeOnOrAfter(originalSwipedAt);
    }
}
