using FluentAssertions;
using NSubstitute;
using Matching.Application.Abstractions;
using Matching.Application.Swipes;
using Matching.Domain.Entities;

namespace Matching.Application.Tests.Swipes;

public class RecordSwipeHandlerTests
{
    private readonly ISwipeRepository _swipeRepository = Substitute.For<ISwipeRepository>();
    private readonly IMatchRepository _matchRepository = Substitute.For<IMatchRepository>();
    private readonly IMatchEventPublisher _matchEventPublisher = Substitute.For<IMatchEventPublisher>();
    private readonly RecordSwipeHandler _handler;

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _householdId = Guid.NewGuid();
    private readonly Guid _recipeId = Guid.NewGuid();
    private readonly Guid _deckId = Guid.NewGuid();

    public RecordSwipeHandlerTests()
    {
        _handler = new RecordSwipeHandler(_swipeRepository, _matchRepository, _matchEventPublisher);
    }

    private void SetUpSwipesForRecipe(IReadOnlyCollection<Swipe> swipes) =>
        _swipeRepository.GetForRecipeAsync(_householdId, _recipeId, _deckId)
            .Returns(Task.FromResult<IReadOnlyCollection<Swipe>?>(swipes));

    [Fact]
    public async Task ExecuteAsync_WithNoExistingSwipe_CreatesNewSwipe()
    {
        _swipeRepository.GetAsync(_userId, _householdId, _recipeId, _deckId).Returns(Task.FromResult<Swipe?>(null));
        SetUpSwipesForRecipe(Array.Empty<Swipe>());

        await _handler.ExecuteAsync(_userId, _householdId, _recipeId, _deckId, liked: true);

        await _swipeRepository.Received(1).AddAsync(Arg.Is<Swipe>(s =>
            s.UserId == _userId &&
            s.HouseholdId == _householdId &&
            s.RecipeId == _recipeId &&
            s.DeckId == _deckId &&
            s.Liked));
        await _swipeRepository.DidNotReceive().UpdateAsync(Arg.Any<Swipe>());
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingSwipe_UpdatesDecisionInstead()
    {
        var existingSwipe = Swipe.Create(_userId, _householdId, _recipeId, _deckId, liked: false);
        _swipeRepository.GetAsync(_userId, _householdId, _recipeId, _deckId).Returns(Task.FromResult<Swipe?>(existingSwipe));
        SetUpSwipesForRecipe(new[] { existingSwipe });

        await _handler.ExecuteAsync(_userId, _householdId, _recipeId, _deckId, liked: true);

        existingSwipe.Liked.Should().BeTrue();
        await _swipeRepository.Received(1).UpdateAsync(existingSwipe);
        await _swipeRepository.DidNotReceive().AddAsync(Arg.Any<Swipe>());
    }

    [Fact]
    public async Task ExecuteAsync_WithOnlyOneUserLiking_ReturnsNotMatchedAndDoesNotTouchMatchRepository()
    {
        _swipeRepository.GetAsync(_userId, _householdId, _recipeId, _deckId).Returns(Task.FromResult<Swipe?>(null));
        SetUpSwipesForRecipe(new[] { Swipe.Create(_userId, _householdId, _recipeId, _deckId, liked: true) });

        var result = await _handler.ExecuteAsync(_userId, _householdId, _recipeId, _deckId, liked: true);

        result.Matched.Should().BeFalse();
        await _matchRepository.DidNotReceive().GetAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>());
        await _matchRepository.DidNotReceive().AddAsync(Arg.Any<Match>());
        await _matchEventPublisher.DidNotReceive().PublishMatchCreatedAsync(Arg.Any<Match>());
    }

    [Fact]
    public async Task ExecuteAsync_WithTwoDistinctUsersLikingAndNoExistingMatch_CreatesMatchAndPublishesEvent()
    {
        _swipeRepository.GetAsync(_userId, _householdId, _recipeId, _deckId).Returns(Task.FromResult<Swipe?>(null));
        SetUpSwipesForRecipe(new[]
        {
            Swipe.Create(_userId, _householdId, _recipeId, _deckId, liked: true),
            Swipe.Create(Guid.NewGuid(), _householdId, _recipeId, _deckId, liked: true),
        });
        _matchRepository.GetAsync(_householdId, _recipeId, _deckId).Returns(Task.FromResult<Match?>(null));

        var result = await _handler.ExecuteAsync(_userId, _householdId, _recipeId, _deckId, liked: true);

        result.Matched.Should().BeTrue();
        await _matchRepository.Received(1).AddAsync(Arg.Is<Match>(m =>
            m.HouseholdId == _householdId &&
            m.RecipeId == _recipeId &&
            m.DeckId == _deckId));
        await _matchEventPublisher.Received(1).PublishMatchCreatedAsync(Arg.Any<Match>());
    }

    [Fact]
    public async Task ExecuteAsync_WithTwoDistinctUsersLikingAndMatchAlreadyExists_DoesNotCreateDuplicateMatch()
    {
        _swipeRepository.GetAsync(_userId, _householdId, _recipeId, _deckId).Returns(Task.FromResult<Swipe?>(null));
        SetUpSwipesForRecipe(new[]
        {
            Swipe.Create(_userId, _householdId, _recipeId, _deckId, liked: true),
            Swipe.Create(Guid.NewGuid(), _householdId, _recipeId, _deckId, liked: true),
        });
        var existingMatch = Match.Create(_householdId, _recipeId, _deckId);
        _matchRepository.GetAsync(_householdId, _recipeId, _deckId).Returns(Task.FromResult<Match?>(existingMatch));

        var result = await _handler.ExecuteAsync(_userId, _householdId, _recipeId, _deckId, liked: true);

        result.Matched.Should().BeTrue();
        await _matchRepository.DidNotReceive().AddAsync(Arg.Any<Match>());
        await _matchEventPublisher.DidNotReceive().PublishMatchCreatedAsync(Arg.Any<Match>());
    }
}
