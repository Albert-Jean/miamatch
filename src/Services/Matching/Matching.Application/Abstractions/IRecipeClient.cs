using System;
using System.Threading.Tasks;

namespace Matching.Application.Abstractions
{
    public interface IRecipeClient
    {
        Task<RecipeDetails?> GetRecipeAsync(Guid recipeId);
        Task<DeckSummary?> GetDeckAsync(Guid deckId);
    }

    public sealed record RecipeDetails(Guid Id, string Name, string ImageUrl);
    public sealed record DeckSummary(Guid Id, Guid HouseholdId, int MealCount, int RecipeCount);
}
