using Microsoft.EntityFrameworkCore;
using WebMonitor.Attributes;
using WebMonitor.Model;

namespace WebMonitor.Middleware;

public class AllowedFeatureMiddleware
{
    private readonly RequestDelegate _next;

    public AllowedFeatureMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint is null)
        {
            await _next(context);
            return;
        }

        var metadata = endpoint.Metadata;

        var featureGuard = metadata.GetMetadata<FeatureGuardAttribute>();
        if (featureGuard is not null)
        {
            // If the user is not logged in or is not allowed to access the feature, return 403
            if (!featureGuard.GetValue(await GetAllowedFeaturesAsync(context)))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }
        }

        await _next(context);
    }

    private static async Task<AllowedFeatures> GetAllowedFeaturesAsync(HttpContext context)
    {
        if (context.User.Identity is null)
            return AllowedFeatures.None;

        await using var db = new WebMonitorContext();
        var users = await db.Users.Include(u => u.AllowedFeatures)
            .Where(u => u.Username == context.User.Identity.Name).ToListAsync();

        return users.Count != 1 ? AllowedFeatures.None : users[0].AllowedFeatures;
    }
}

public static class AllowedFeatureMiddlewareExtensions
{
    public static IApplicationBuilder UseAllowedFeatureMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AllowedFeatureMiddleware>();
    }
}
