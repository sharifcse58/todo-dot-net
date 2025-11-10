using Microsoft.AspNetCore.Http;

namespace MyApiProject.Middleware;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private const string HeaderName = "x-api-key";

    public ApiKeyMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, IConfiguration config)
    {
        // Allow Swagger & static files
        var path = context.Request.Path.Value ?? string.Empty;
        if (path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // Only protect non-GET requests (CRUD writes)
        if (!HttpMethods.IsGet(context.Request.Method))
        {
            var supplied = context.Request.Headers[HeaderName].FirstOrDefault();
            var expected = config["Auth:StaticApiKey"];

            if (string.IsNullOrWhiteSpace(expected) || supplied != expected)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Unauthorized",
                    detail = $"Provide header '{HeaderName}'."
                });
                return;
            }
        }

        await _next(context);
    }
}

public static class ApiKeyMiddlewareExtensions
{
    public static IApplicationBuilder UseStaticApiKeyAuth(this IApplicationBuilder app) =>
        app.UseMiddleware<ApiKeyMiddleware>();
}
