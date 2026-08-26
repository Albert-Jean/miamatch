using System.Security.Claims;
using ShoppingList.Application.Abstractions;

namespace ShoppingList.Api.Endpoints
{
    public static class ShoppingListEndpoints
    {
        public static void MapShoppingListEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/shopping-list", async (Guid householdId, ClaimsPrincipal user, IShoppingListItemRepository repository) =>
            {
                var householdIds = user.Claims.Where(c => c.Type == "householdId").Select(c => Guid.Parse(c.Value));
                if (!householdIds.Contains(householdId))
                {
                    return Results.Forbid();
                }

                var items = await repository.GetForHouseholdAsync(householdId);
                var grouped = items
                    .GroupBy(i => i.IngredientName)
                    .Select(g => new { IngredientName = g.Key, Measures = g.Select(i => i.Measure).ToList() });
                return Results.Ok(grouped);
            }).RequireAuthorization();
        }
    }
}
