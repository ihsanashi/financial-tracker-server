using Scalar.AspNetCore;
using server.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Load user secrets
builder.Configuration.AddUserSecrets<Program>();

var app = builder.Build();

app.MapWhen(
    context => context.Request.Path.StartsWithSegments("/api/v1"),
    appBuilder =>
    {
        appBuilder.UseSupabaseAuth();
    }
);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference(); // scalar/v1
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
