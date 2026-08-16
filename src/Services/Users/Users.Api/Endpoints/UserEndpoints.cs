using System.Security.Claims;
using Users.Application.Abstractions;
using Users.Application.Users;

namespace Users.Api.Endpoints
{
    public static class UserEndpoints
    {
        public static void MapUserEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/users", async (RegisterUserRequest request, RegisterUserHandler handler) =>
            {
                var result = await handler.ExecuteAsync(request.Name, request.Email, request.Password);
                return Results.Created($"/users/{result.UserId}", result);
            });
            app.MapPost("/auth/login", async (LoginUserRequest request, LoginHandler handler) =>
            {
                var result = await handler.ExecuteAsync(request.Email, request.Password);
                return Results.Ok(result);
            });
            app.MapGet("/me", async (ClaimsPrincipal user, IUserRepository userRepository, IHouseholdRepository householdRepository) =>
            {
                var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

                var currentUser = await userRepository.GetByIdAsync(userId);
                if (currentUser is null)
                {
                    return Results.NotFound();
                }

                var householdIds = await householdRepository.GetHouseholdIdsForUserAsync(userId);

                return Results.Ok(new
                {
                    currentUser.Id,
                    currentUser.Name,
                    Email = currentUser.Email.EmailAddress,
                    HouseholdIds = householdIds
                });
            }).RequireAuthorization();
        }
    }
    public sealed record RegisterUserRequest(string Name, string Email, string Password);
    public sealed record LoginUserRequest(string Email, string Password);
}
