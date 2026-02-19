namespace ApiIngesol.Helpers;

public static class MiddlewareHelper
{
    public static void UseSwaggerRootRedirect(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            if (context.Request.Path == "/")
            {
                context.Response.Redirect("/swagger");
                return;
            }
            await next();
        });
    }
}
