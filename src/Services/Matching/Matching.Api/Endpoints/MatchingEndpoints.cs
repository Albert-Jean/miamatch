using System.Security.Claims;
using Matching.Application.Abstractions;
using Matching.Application.Swipes;

namespace Matching.Api.Endpoints
{
    public static class MatchingEndpoints
    {
        public static void MapMatchingEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/swipes", async (CreateSwipeRequest request, ClaimsPrincipal user, RecordSwipeHandler handler) =>
            {
                var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var householdsIds = user.Claims.Where(c => c.Type == "householdId").Select(c => Guid.Parse(c.Value));
            if (householdsIds.Contains(request.HouseholdId))
            {
                var result = await handler.ExecuteAsync(userId, request.HouseholdId, request.RecipeId, request.DeckId, request.Liked);
                return Results.Ok(result);
            }
            else
            {
                return Results.Forbid();
                }
            }
            ).RequireAuthorization();
            app.MapGet("/matches", async (Guid householdId, ClaimsPrincipal user, IMatchRepository matchRepository, IRecipeClient recipeClient) =>
            {
                var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var householdsIds = user.Claims.Where(c => c.Type == "householdId").Select(c => Guid.Parse(c.Value));
                if (householdsIds.Contains(householdId))
                {
                    var matches = await matchRepository.GetForHouseholdAsync(householdId);
                    // Matching only stores recipe ids, so each match is enriched with the
                    // name/image Recipes.Api owns before returning it to the frontend.
                    var enriched = await Task.WhenAll(matches.Select(async m =>
                    {
                        var recipe = await recipeClient.GetRecipeAsync(m.RecipeId);
                        return new MatchResponse(m.RecipeId, recipe?.Name ?? "", recipe?.ImageUrl ?? "", m.MatchedAt);
                    }));
                    return Results.Ok(enriched);
                }
                else
                {
                    return Results.Forbid();
                }
            }).RequireAuthorization();
        }
    }
    public sealed record CreateSwipeRequest(Guid HouseholdId, Guid RecipeId, Guid DeckId, bool Liked);
    public sealed record MatchResponse(Guid RecipeId, string Name, string ImageUrl, DateTime MatchedAt);
}
