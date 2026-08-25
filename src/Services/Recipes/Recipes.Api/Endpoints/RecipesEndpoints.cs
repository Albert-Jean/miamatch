using System.Security.Claims;
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
        }
    }
}
