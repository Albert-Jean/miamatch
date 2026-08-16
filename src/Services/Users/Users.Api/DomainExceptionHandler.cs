using Microsoft.AspNetCore.Diagnostics;
using Users.Domain.Exceptions;

namespace Users.Api
{
    public class DomainExceptionHandler: IExceptionHandler
    {
       public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if(exception is not DomainException domainException)
            {
                return false;
            }
            else
            {
                int statusCode = domainException switch
                {
                    HouseholdFullException or AlreadyMemberException or EmailAlreadyInUseException => StatusCodes.Status409Conflict,
                    InvalidEmailException or InvalidInviteCodeException => StatusCodes.Status400BadRequest,
                    InvalidCredentialsException => StatusCodes.Status401Unauthorized,
                    HouseHoldNotFoundException => StatusCodes.Status404NotFound,
                    _ => StatusCodes.Status400BadRequest
                };

                httpContext.Response.StatusCode = statusCode;
                await httpContext.Response.WriteAsJsonAsync(new { error = domainException.Message }, cancellationToken);

                return true;
            }
        }
    }
}
