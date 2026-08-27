using System;
using System.Collections.Generic;
using System.Text;
using Recipes.Application.Abstractions;

namespace Recipes.Application.Decks
{
    public class GetCurrentDeckHandler
    {
        private readonly IDeckRepository _deckRepository;
        private readonly IRecipeRepository _recipeRepository;
        public GetCurrentDeckHandler(IDeckRepository deckRepository, IRecipeRepository recipeRepository)
        {
            _deckRepository = deckRepository;
            _recipeRepository = recipeRepository;
        }
        public async Task<DeckResult?> ExecuteAsync(Guid householdId)
        {
            var deck = await _deckRepository.GetMostRecentAsync(householdId);
            if (deck is null || deck.IsExpired(DateTime.UtcNow))
            {
                return null;
            }

            var recipes = await _recipeRepository.GetByIdsAsync(deck.RecipeIds);
            var summaries = recipes.Select(r => new RecipeSummary(r.Id, r.Name, r.ImageUrl, r.Tags)).ToList();
            return new DeckResult(deck.Id, summaries);
        }
    }
    public sealed record DeckResult(Guid DeckId, IReadOnlyCollection<RecipeSummary> Recipes);
    public sealed record RecipeSummary(Guid Id, string Name, string ImageUrl, IReadOnlyCollection<string> Tags);
}
