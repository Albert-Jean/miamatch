using FluentAssertions;
using NSubstitute;
using Matching.Application.Abstractions;
using Matching.Application.Swipes;
using Matching.Domain.Entities;

namespace Matching.Application.Tests.Swipes;

public class GetDeckSwipeStateHandlerTests
{
    private readonly ISwipeRepository _swipeRepository = Substitute.For<ISwipeRepository>();
    private readonly IMatchRepository _matchRepository = Substitute.For<IMatchRepository>();
    private readonly IRecipeClient _recipeClient = Substitute.For<IRecipeClient>();
    private readonly GetDeckSwipeStateHandler _handler;

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _householdId = Guid.NewGuid();
    private readonly Guid _deckId = Guid.NewGuid();

    public GetDeckSwipeStateHandlerTests()
    {
        _handler = new GetDeckSwipeStateHandler(_swipeRepository, _matchRepository, _recipeClient);
        _recipeClient.GetDeckAsync(_deckId).Returns(Task.FromResult<DeckSummary?>(new DeckSummary(_deckId, _householdId, 5, 20)));
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsTheRecipesTheUserAlreadySwiped()
    {
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        _swipeRepository.GetForUserAndDeckAsync(_userId, _householdId, _deckId).Returns(
            Task.FromResult<IReadOnlyCollection<Swipe>>(new[]
            {
                Swipe.Create(_userId, _householdId, first, _deckId, liked: true),
                Swipe.Create(_userId, _householdId, second, _deckId, liked: false),
            }));

        var state = await _handler.ExecuteAsync(_userId, _householdId, _deckId);

        state.SwipedRecipeIds.Should().BeEquivalentTo(new[] { first, second });
        state.MealCount.Should().Be(5);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoSwipeYet_ReturnsAnEmptyResumePoint()
    {
        _swipeRepository.GetForUserAndDeckAsync(_userId, _householdId, _deckId)
            .Returns(Task.FromResult<IReadOnlyCollection<Swipe>>(Array.Empty<Swipe>()));

        var state = await _handler.ExecuteAsync(_userId, _householdId, _deckId);

        state.SwipedRecipeIds.Should().BeEmpty();
        state.WeekComplete.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheMealCountIsReached_ReportsTheWeekComplete()
    {
        _swipeRepository.GetForUserAndDeckAsync(_userId, _householdId, _deckId)
            .Returns(Task.FromResult<IReadOnlyCollection<Swipe>>(Array.Empty<Swipe>()));
        _matchRepository.CountForDeckAsync(_householdId, _deckId).Returns(Task.FromResult(5));

        var state = await _handler.ExecuteAsync(_userId, _householdId, _deckId);

        state.MatchCount.Should().Be(5);
        state.WeekComplete.Should().BeTrue();
    }
}
