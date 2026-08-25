using System;
using System.Collections.Generic;
using System.Text;

namespace ShoppingList.Application.Abstractions
{
    public interface IRecipeClient
    {
        Task<RecipeDetails?> GetRecipeAsync(Guid recipeId);
    }

    public sealed record RecipeDetails(Guid Id, string Name, IReadOnlyCollection<RecipeIngredientDto> Ingredients);
    public sealed record RecipeIngredientDto(string Name, string Measure);
}
