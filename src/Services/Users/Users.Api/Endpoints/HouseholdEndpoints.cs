using System.Security.Claims;
using Users.Application.Abstractions;
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
                return Results.Created($"/households/{result.HouseholdId}", result);
            }).RequireAuthorization();
            app.MapPost("/households/join", async (JoinHouseholdRequest request, ClaimsPrincipal user, JoinHouseholdHandler handler) =>
            {
                var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await handler.ExecuteAsync(request.InviteCode, userId);
                return Results.Ok(result);
            }).RequireAuthorization();
            app.MapGet("/households/{id}/members", async (Guid id, IHouseholdRepository householdRepository) =>
            {
                var household = await householdRepository.GetByIdAsync(id);
                if (household is null)
                {
                    return Results.NotFound();
                }

                var memberIds = household.Members.Select(m => m.UserId).ToList();
                return Results.Ok(memberIds);
            });
            app.MapGet("/households/{id}", async (Guid id, IHouseholdRepository householdRepository) =>
            {
                var household = await householdRepository.GetByIdAsync(id);
                if (household is null)
                {
                    return Results.NotFound();
                }

                return Results.Ok(new
                {
                    HouseholdId = household.Id,
                    household.Name,
                    InviteCode = household.InviteCode.Value,
                    MemberCount = household.Members.Count
                });
            }).RequireAuthorization();
        }
    }
    public sealed record CreateHouseholdRequest(string Name);
    public sealed record JoinHouseholdRequest(string InviteCode);
}
