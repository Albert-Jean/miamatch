using FluentAssertions;
using Matching.Domain.Entities;
using Matching.Domain.Services;

namespace Matching.Domain.Tests.Services;

public class MatchEvaluatorTests
{
    private static Swipe CreateSwipe(Guid userId, bool liked) =>
        Swipe.Create(userId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), liked);

    [Fact]
    public void IsMatch_WithNoSwipes_ReturnsFalse()
    {
        MatchEvaluator.IsMatch(Array.Empty<Swipe>()).Should().BeFalse();
    }

    [Fact]
    public void IsMatch_WithOnlyOneUserLiking_ReturnsFalse()
    {
        var swipes = new[] { CreateSwipe(Guid.NewGuid(), liked: true) };

        MatchEvaluator.IsMatch(swipes).Should().BeFalse();
    }

    [Fact]
    public void IsMatch_WithTwoDistinctUsersLiking_ReturnsTrue()
    {
        var swipes = new[]
        {
            CreateSwipe(Guid.NewGuid(), liked: true),
            CreateSwipe(Guid.NewGuid(), liked: true),
        };

        MatchEvaluator.IsMatch(swipes).Should().BeTrue();
    }

    [Fact]
    public void IsMatch_WithSameUserLikingTwice_ReturnsFalse()
    {
        var userId = Guid.NewGuid();
        var swipes = new[]
        {
            CreateSwipe(userId, liked: true),
            CreateSwipe(userId, liked: true),
        };

        MatchEvaluator.IsMatch(swipes).Should().BeFalse();
    }

    [Fact]
    public void IsMatch_IgnoresDislikes()
    {
        var swipes = new[]
        {
            CreateSwipe(Guid.NewGuid(), liked: true),
            CreateSwipe(Guid.NewGuid(), liked: false),
        };

        MatchEvaluator.IsMatch(swipes).Should().BeFalse();
    }
}
