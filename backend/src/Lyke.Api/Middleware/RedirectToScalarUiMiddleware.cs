namespace Lyke.Api.Middleware;

public class RedirectToScalarUiMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Path == "/")
        {
            context.Response.Redirect("/scalar/be-lyke_api_v1");
        }
        else
        {
            await next(context);
        }
    }
}