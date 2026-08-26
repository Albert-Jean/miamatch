using System.Security.Claims;
using Notifications.Application.PushSubscriptions;

namespace Notifications.Api.Endpoints;

public static class NotificationsEndpoints
{
    public static void MapNotificationsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/vapid", (IConfiguration configuration) =>
        {
            return Results.Ok(new { publicKey = configuration["Vapid:PublicKey"] });
        });

        app.MapPost("/push/subscribe", async (SubscribeRequest request, ClaimsPrincipal user, RegisterPushSubscriptionHandler handler) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await handler.ExecuteAsync(userId, request.Endpoint, request.P256dhKey, request.AuthKey);
            return Results.Ok();
        }).RequireAuthorization();
    }
}

public sealed record SubscribeRequest(string Endpoint, string P256dhKey, string AuthKey);
