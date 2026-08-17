using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Matching.Application.Abstractions;
using Matching.Domain.Entities;
using Matching.Domain.Services;

namespace Matching.Application.Swipes
{
    public class RecordSwipeHandler
    {
        private readonly ISwipeRepository _swipeRepository;
        private readonly IMatchRepository _matchRepository;
        private readonly IMatchEventPublisher _matchEventPublisher;

        public RecordSwipeHandler(ISwipeRepository swipeRepository, IMatchRepository matchRepository, IMatchEventPublisher matchEventPublisher)
        {
            _swipeRepository = swipeRepository;
            _matchRepository = matchRepository;
            _matchEventPublisher = matchEventPublisher;
        }

        public async Task<RecordSwipeResult> ExecuteAsync(Guid userId, Guid householdId,Guid recipeId, Guid deckId,bool liked)
        {
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
                return new RecordSwipeResult(true);
            }
            return new RecordSwipeResult(false);
        }
    }

    public sealed record RecordSwipeResult(bool Matched);
}
