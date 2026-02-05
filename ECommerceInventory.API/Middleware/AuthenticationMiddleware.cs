using ECommerceInventory.Application.Interfaces.Repositories;
using ECommerceInventory.Infrastructure.Security;

namespace ECommerceInventory.API.Middleware;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public AuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITokenGenerator tokenGenerator, ISessionRepository sessionRepository)
    {
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();

        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var token = authHeader["Bearer ".Length..].Trim();

            var payload = tokenGenerator.ValidateToken(token);
            if (payload != null && payload.ExpiresAt > DateTime.UtcNow)
            {
                var session = await sessionRepository.GetByIdAsync(payload.SessionId);
                if (session != null && !session.IsRevoked && session.ExpiresAt > DateTime.UtcNow)
                {
                    context.Items["UserId"] = payload.UserId;
                    context.Items["SessionId"] = payload.SessionId;
                    context.Items["Token"] = token;
                }
            }
        }

        await _next(context);
    }
}

public static class AuthenticationMiddlewareExtensions
{
    public static IApplicationBuilder UseTokenAuthentication(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AuthenticationMiddleware>();
    }
}
