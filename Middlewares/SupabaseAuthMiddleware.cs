using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Supabase;
using System.Threading.Tasks;

namespace server.Middlewares;

public class SupabaseAuthMiddleware
{
  private readonly RequestDelegate _next;
  private readonly Supabase.Client _supabaseClient;

  public SupabaseAuthMiddleware(RequestDelegate next, IConfiguration configuration)
  {
    _next = next;

    var supabaseUrl = configuration["Supabase:Url"];
    var supabaseSecret = configuration["Supabase:Secret"];

    if (string.IsNullOrEmpty(supabaseUrl) || string.IsNullOrEmpty(supabaseSecret))
    {
      throw new Exception("Supabase credentials are missing.");
    }

    _supabaseClient = new Supabase.Client(supabaseUrl, supabaseSecret);
  }

  public async Task InvokeAsync(HttpContext context)
  {
    var token = context.Request.Headers.Authorization.ToString().Replace("Bearer ", "");

    if (string.IsNullOrEmpty(token))
    {
      context.Response.StatusCode = StatusCodes.Status401Unauthorized;
      await context.Response.WriteAsync("Unauthorized, no token provided");
      return;
    }

    try
    {
      var user = await _supabaseClient.Auth.GetUser(token);

      if (user == null)
      {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsync("Unauthorized, invalid token");
        return;
      }

      context.Items["User"] = user;

      await _next(context);
    }
    catch (Exception)
    {
      context.Response.StatusCode = StatusCodes.Status500InternalServerError;
      await context.Response.WriteAsync("Exception running Supabase Auth middleware");
    }
  }
}

public static class SupabaseAuthMiddlewareExtensions
{
  public static IApplicationBuilder UseSupabaseAuth(this IApplicationBuilder builder)
  {
    return builder.UseMiddleware<SupabaseAuthMiddleware>();
  }
}
