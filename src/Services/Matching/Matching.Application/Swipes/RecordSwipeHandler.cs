using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Matching.Application.Abstractions;
using Matching.Domain.Entities;
using Matching.Domain.Exceptions;
using Matching.Domain.Services;

namespace Matching.Application.Swipes
{
    public class RecordSwipeHandler
    {
        private readonly ISwipeRepository _swipeRepository;
        private readonly IMatchRepository _matchRepository;
        private readonly IMatchEventPublisher _matchEventPublisher;
        private readonly IRecipeClient _recipeClient;

        public RecordSwipeHandler(ISwipeRepository swipeRepository, IMatchRepository matchRepository, IMatchEventPublisher matchEventPublisher, IRecipeClient recipeClient)
        {
            _swipeRepository = swipeRepository;
            _matchRepository = matchRepository;
            _matchEventPublisher = matchEventPublisher;
            _recipeClient = recipeClient;
        }

        public async Task<RecordSwipeResult> ExecuteAsync(Guid userId, Guid householdId,Guid recipeId, Guid deckId,bool liked)
        {
            // A deck we cannot read leaves mealCount at 0, which disables the limit rather than
            // blocking every swipe while the Recipes service is unreachable.
            var deck = await _recipeClient.GetDeckAsync(deckId);
            var mealCount = deck?.MealCount ?? 0;

            if (mealCount > 0 && await _matchRepository.CountForDeckAsync(householdId, deckId) >= mealCount)
            {
                throw new WeekAlreadyPlannedException(deckId, mealCount);
            }

            var existingSwipe = await _swipeRepository.GetAsync(userId, householdId, recipeId, deckId);
            if ( existingSwipe!= null)
            {
                existingSwipe.ChangeDecision(liked);
                await _swipeRepository.UpdateAsync(existingSwipe);
            }
            else
            {
                var newSwipe = Swipe.Create(userId, householdId, recipeId, deckId, liked);
                await _swipeRepository.AddAsync(newSwipe);
            }
            var swipes = await _swipeRepository.GetForRecipeAsync(householdId,recipeId,deckId);
            bool isMatch = MatchEvaluator.IsMatch(swipes);
            if(isMatch)
            {
                if(await _matchRepository.GetAsync(householdId, recipeId, deckId)is null)
                {
                    Match match = Match.Create(householdId, recipeId, deckId);
                    await _matchRepository.AddAsync(match);
                    await _matchEventPublisher.PublishMatchCreatedAsync(match);
                }                
            }

            var matchCount = await _matchRepository.CountForDeckAsync(householdId, deckId);
            return new RecordSwipeResult(isMatch, matchCount, mealCount, mealCount > 0 && matchCount >= mealCount);
        }
    }

    public sealed record RecordSwipeResult(bool Matched, int MatchCount, int MealCount, bool WeekComplete);
}
