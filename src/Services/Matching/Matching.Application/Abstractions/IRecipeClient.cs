using System;
using System.Threading.Tasks;

namespace Matching.Application.Abstractions
{
    public interface IRecipeClient
    {
        Task<RecipeDetails?> GetRecipeAsync(Guid recipeId);
    }

    public sealed record RecipeDetails(Guid Id, string Name, string ImageUrl);
}
