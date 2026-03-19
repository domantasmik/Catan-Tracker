using Microsoft.AspNetCore.Mvc;
using Catan.exceptions;
using System.Net;

namespace Catan.middleware;
public class ExceptionHandlingMiddleware
{
    // readonly
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
            // HttpStatusCode.InternalServerError
            // Visur kur StatusCode grazini naudok geriau HttpStatusCode klase.
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new {error ="An unexpected error occurred"});
        }
    }
}