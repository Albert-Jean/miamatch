using System.Security.Claims;
using Recipes.Application.Abstractions;
using Recipes.Application.Decks;

namespace Recipes.Api.Endpoints
{
    public static class RecipesEndpoints
    {
        public static void MapRecipesEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/decks/current", async (Guid householdId, ClaimsPrincipal user, GetCurrentDeckHandler handler) =>
            {
                if (!UserBelongsToHousehold(user, householdId))
                {
                    return Results.Forbid();
                }

                var result = await handler.ExecuteAsync(householdId);
                return result is null ? Results.NotFound() : Results.Ok(result);
            }).RequireAuthorization();

            app.MapPost("/decks", async (GenerateDeckRequest request, ClaimsPrincipal user, GenerateDeckHandler handler) =>
            {
                if (!UserBelongsToHousehold(user, request.HouseholdId))
                {
                    return Results.Forbid();
                }

                var result = await handler.ExecuteAsync(request.HouseholdId, request.Categories ?? Array.Empty<string>());
                return Results.Ok(result);
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
                    recipe.Tags,
                    recipe.Ingredients.Select(i => new RecipeIngredientResponse(i.name, i.measure)).ToList(),
                    recipe.Instructions.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

                return Results.Ok(response);
            });
        }

        private static bool UserBelongsToHousehold(ClaimsPrincipal user, Guid householdId) =>
            user.Claims.Where(c => c.Type == "householdId").Select(c => Guid.Parse(c.Value)).Contains(householdId);
    }
    public sealed record GenerateDeckRequest(Guid HouseholdId, string[]? Categories);
    public sealed record RecipeDetailsResponse(Guid Id, string Name, string ImageUrl, IReadOnlyCollection<string> Tags, IReadOnlyCollection<RecipeIngredientResponse> Ingredients, IReadOnlyCollection<string> Steps);
    public sealed record RecipeIngredientResponse(string Name, string Measure);
}
