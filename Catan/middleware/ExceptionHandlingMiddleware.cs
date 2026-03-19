using Microsoft.AspNetCore.Mvc;
using Catan.exceptions;

namespace Catan.middleware;
public class ExceptionHandlingMiddleware
{
    private RequestDelegate _next;
    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch(AppException ex)
        {
            context.Response.StatusCode = ex.HttpCode;
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch(Exception)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new {error ="An unexpected error occurred"});
        }
    }
}