using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Matching.Application.Abstractions;

namespace Matching.Application.Swipes
{
    /// <summary>
    /// Tells the client where the current user left off in a deck, so reopening the swipe tab
    /// resumes instead of dealing the whole deck again, and whether the week is already planned.
    /// </summary>
    public class GetDeckSwipeStateHandler
    {
        private readonly ISwipeRepository _swipeRepository;
        private readonly IMatchRepository _matchRepository;
        private readonly IRecipeClient _recipeClient;

        public GetDeckSwipeStateHandler(ISwipeRepository swipeRepository, IMatchRepository matchRepository, IRecipeClient recipeClient)
        {
            _swipeRepository = swipeRepository;
            _matchRepository = matchRepository;
            _recipeClient = recipeClient;
        }

        public async Task<DeckSwipeState> ExecuteAsync(Guid userId, Guid householdId, Guid deckId)
        {
            var swipes = await _swipeRepository.GetForUserAndDeckAsync(userId, householdId, deckId);
            var deck = await _recipeClient.GetDeckAsync(deckId);
            var mealCount = deck?.MealCount ?? 0;
            var matchCount = await _matchRepository.CountForDeckAsync(householdId, deckId);

            return new DeckSwipeState(
                deckId,
                swipes.Select(s => s.RecipeId).ToList(),
                matchCount,
                mealCount,
                mealCount > 0 && matchCount >= mealCount);
        }
    }

    public sealed record DeckSwipeState(Guid DeckId, IReadOnlyCollection<Guid> SwipedRecipeIds, int MatchCount, int MealCount, bool WeekComplete);
}
