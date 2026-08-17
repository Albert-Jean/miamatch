using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Recipe.Domain.Entities;
using Recipes.Application.Abstractions;
using Recipes.Domain.Services;

namespace Recipes.Application.Decks
{
    public class GetOrGenerateDeckHandler
    {
        private readonly IDeckRepository _deckRepository;
        private readonly IRecipeRepository _recipeRepository;
        private readonly IRecipeCatalogClient _recipeCatalogClient;
        public GetOrGenerateDeckHandler(IDeckRepository deckRepository, IRecipeRepository recipeRepository, IRecipeCatalogClient recipeCatalogClient)
        {
            _deckRepository = deckRepository;
            _recipeRepository = recipeRepository;
            _recipeCatalogClient = recipeCatalogClient;
        }
        public async Task<GetOrGenerateDeckResult> ExecuteAsync(Guid householdId)
        {
            var existingDeck = await _deckRepository.GetMostRecentAsync(householdId);
            if (existingDeck is not null && !existingDeck.IsExpired(DateTime.UtcNow))
            {
                var recipes = await _recipeRepository.GetByIdsAsync(existingDeck.RecipesId);
                var summaries = recipes.Select(r => new RecipeSummary(r.Id, r.Name, r.ImageUrl)).ToList();
                return new GetOrGenerateDeckResult(existingDeck.Id, summaries);
            }
            else
            {
                var pool = (await _recipeRepository.GetAllAsync()).ToList();

                if (pool.Count < 20)
                {
                    var existingMealDbIds = pool.Select(r => r.MealDbId).ToHashSet();
                    var newRecipes = new List<Recipes.Domain.Entities.Recipe>();

                    while (pool.Count + newRecipes.Count < 20)
                    {
                        var fetched = await _recipeCatalogClient.FetchRecipesAsync(20 - (pool.Count + newRecipes.Count));
                        foreach (var recipe in fetched)
                        {
                            if (!existingMealDbIds.Contains(recipe.MealDbId) && newRecipes.All(r => r.MealDbId != recipe.MealDbId))
                            {
                                newRecipes.Add(recipe);
                            }
                        }
                    }

                    await _recipeRepository.AddRangeAsync(newRecipes);
                    pool.AddRange(newRecipes);
                }

                var excludedRecipeIds = existingDeck?.RecipesId ?? Array.Empty<Guid>();
                var recipeIds = DeckGenerator.GenerateRecipeIds(pool.Select(r => r.Id).ToList(), excludedRecipeIds.ToList(), 20);

                var deck = Deck.Create(householdId, recipeIds);
                await _deckRepository.AddASync(deck);

                var selectedRecipes = pool.Where(r => recipeIds.Contains(r.Id));
                return new GetOrGenerateDeckResult(deck.Id, selectedRecipes.Select(r => new RecipeSummary(r.Id, r.Name, r.ImageUrl)).ToList());
            }           
            }
        }
    }
    public sealed record GetOrGenerateDeckResult(Guid DeckId, IReadOnlyCollection<RecipeSummary> Recipes);
    public sealed record RecipeSummary(Guid Id, string Name, string ImageUrl);
