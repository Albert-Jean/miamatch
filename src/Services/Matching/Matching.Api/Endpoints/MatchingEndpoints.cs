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
            app.MapGet("/matches", async (Guid householdId, ClaimsPrincipal user, IMatchRepository matchRepository) =>
            {
                var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var householdsIds = user.Claims.Where(c => c.Type == "householdId").Select(c => Guid.Parse(c.Value));
                if (householdsIds.Contains(householdId))
                {
                    var result = await matchRepository.GetForHouseholdAsync(householdId);
                    return Results.Ok(result);
                }
                else
                {
                    return Results.Forbid();
                }            
            }).RequireAuthorization();
        }
    }
    public sealed record CreateSwipeRequest(Guid HouseholdId, Guid RecipeId, Guid DeckId, bool Liked);
}
