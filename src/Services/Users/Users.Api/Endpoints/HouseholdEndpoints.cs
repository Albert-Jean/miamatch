using System.Security.Claims;
using Users.Application.Households;
using Users.Application.Users;

namespace Users.Api.Endpoints
{
    public static class HouseholdEndpoints
    {
        public static void MapHouseholdEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/households", async (CreateHouseholdRequest request, ClaimsPrincipal user, CreateHouseholdHandler handler) =>
            {
                var creatorUserId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await handler.ExecuteAsync(request.Name, creatorUserId);
                return Results.Created($"/households/{result.HouseholdID}", result);
            }).RequireAuthorization();
            app.MapPost("/households/join", async (JoinHouseholdRequest request, ClaimsPrincipal user, JoinHouseholdHandler handler) =>
            {
                var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await handler.ExecuteAsync(request.InviteCode, userId);
                return Results.Ok(result);
            }).RequireAuthorization();
        }
    }
    public sealed record CreateHouseholdRequest(string Name);
    public sealed record JoinHouseholdRequest(string InviteCode);
}
