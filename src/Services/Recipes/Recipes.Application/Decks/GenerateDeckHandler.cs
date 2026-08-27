using System;
using System.Collections.Generic;
using System.Text;
using Recipe.Domain.Entities;
using Recipes.Application.Abstractions;
using Recipes.Domain.Services;

namespace Recipes.Application.Decks
{
    public class GenerateDeckHandler
    {
        private const int DeckSize = 20;

        private readonly IDeckRepository _deckRepository;
        private readonly IRecipeRepository _recipeRepository;
        private readonly IRecipeCatalog _recipeCatalog;
        public GenerateDeckHandler(IDeckRepository deckRepository, IRecipeRepository recipeRepository, IRecipeCatalog recipeCatalog)
        {
            _deckRepository = deckRepository;
            _recipeRepository = recipeRepository;
            _recipeCatalog = recipeCatalog;
        }
        public async Task<DeckResult> ExecuteAsync(Guid householdId, IReadOnlyCollection<string> categories)
        {
            var existingDeck = await _deckRepository.GetMostRecentAsync(householdId);
            if (existingDeck is not null && !existingDeck.IsExpired(DateTime.UtcNow))
            {
                var currentRecipes = await _recipeRepository.GetByIdsAsync(existingDeck.RecipeIds);
                return new DeckResult(existingDeck.Id, currentRecipes.Select(ToSummary).ToList());
            }

            var pool = (await _recipeRepository.GetAllAsync()).ToList();

            var knownExternalIds = pool.Select(r => r.ExternalId).ToHashSet();
            var newRecipes = (await _recipeCatalog.GetRecipesAsync())
                .Where(r => !knownExternalIds.Contains(r.ExternalId))
                .ToList();
            if (newRecipes.Count > 0)
            {
                await _recipeRepository.AddRangeAsync(newRecipes);
                pool.AddRange(newRecipes);
            }

            var normalizedCategories = categories
                .Select(c => c.Trim().ToLowerInvariant())
                .Where(c => c.Length > 0)
                .ToHashSet();
            var candidates = normalizedCategories.Count == 0
                ? pool
                : pool.Where(r => r.Tags.Any(normalizedCategories.Contains)).ToList();

            var excludedRecipeIds = existingDeck?.RecipeIds ?? Array.Empty<Guid>();
            var recipeIds = DeckGenerator.GenerateRecipeIds(candidates.Select(r => r.Id).ToList(), excludedRecipeIds.ToList(), DeckSize);

            var deck = Deck.Create(householdId, recipeIds);
            await _deckRepository.AddAsync(deck);

            var selectedRecipes = candidates.Where(r => recipeIds.Contains(r.Id));
            return new DeckResult(deck.Id, selectedRecipes.Select(ToSummary).ToList());
        }

        private static RecipeSummary ToSummary(Recipes.Domain.Entities.Recipe recipe) =>
            new(recipe.Id, recipe.Name, recipe.ImageUrl, recipe.Tags);
    }
}
