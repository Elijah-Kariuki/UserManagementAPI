using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// Register controllers
builder.Services.AddControllers();

// Register NSwag OpenAPI document generation
builder.Services.AddOpenApiDocument(settings =>
{
    settings.Title = "User Management API";
    settings.Version = "v1";
    settings.Description = "A simple API for managing users.";
    settings.DocumentName = "v1";
});

var app = builder.Build();

// Error Handling Middleware
app.Use(async (context, next) =>
{
    try
    {
        await next.Invoke();
    }
    catch (Exception)
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"error\":\"Internal server error.\"}");
    }
});

// Authentication Middleware
app.Use(async (context, next) =>
{
    if (!context.Request.Headers.TryGetValue("Authorization", out var token))
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("Unauthorized");
        return;
    }

    var jwt = token.ToString().Replace("Bearer ", "");
    var handler = new JwtSecurityTokenHandler();

    if (!handler.CanReadToken(jwt))
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("Unauthorized");
        return;
    }

    await next.Invoke();
});

// Logging Middleware
app.Use(async (context, next) =>
{
    Console.WriteLine($"Request: {context.Request.Method} {context.Request.Path}");

    await next.Invoke();

    Console.WriteLine($"Response Status: {context.Response.StatusCode}");
});

// Enable NSwag middleware
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "User Management API v1");
    });
}

app.UseHttpsRedirection();
app.MapControllers();
await app.RunAsync();
