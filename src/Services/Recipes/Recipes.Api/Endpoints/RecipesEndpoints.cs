using System.Security.Claims;
using Recipes.Application.Abstractions;
using Recipes.Application.Decks;

namespace Recipes.Api.Endpoints
{
    public static class RecipesEndpoints
    {
        public static void MapRecipesEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/decks/current", async (Guid householdId, ClaimsPrincipal user, GetOrGenerateDeckHandler handler) =>
            {
                var householdsIds = user.Claims.Where(c => c.Type == "householdId").Select(c => Guid.Parse(c.Value));
                if (householdsIds.Contains(householdId))
                {
                    var result = await handler.ExecuteAsync(householdId);
                    return Results.Ok(result);
                }
                else
                {
                    return Results.Forbid();
                }
            }).RequireAuthorization();
            app.MapGet("/recipes/{id}", async (Guid id, IRecipeRepository recipeRepository) =>
            {
                var recipe = await recipeRepository.GetByIdAsync(id);
                if (recipe is null)
                {
                    return Results.NotFound();
                }

                var response = new RecipeDetailsResponse(
                    recipe.Id,
                    recipe.Name,
                    recipe.ImageUrl,
                    recipe.Ingredients.Select(i => new RecipeIngredientResponse(i.name, i.measure)).ToList());

                return Results.Ok(response);
            });
        }
    }
    public sealed record RecipeDetailsResponse(Guid Id, string Name, string ImageUrl, IReadOnlyCollection<RecipeIngredientResponse> Ingredients);
    public sealed record RecipeIngredientResponse(string Name, string Measure);
}
