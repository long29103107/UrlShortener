// Program.cs

using MediatR;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Api.Models;
using UrlShortener.Application.Contracts.Repositories;
using UrlShortener.Application.Features.UrlShortening.Commands;
using UrlShortener.Application.Services;
using UrlShortener.Infrastructure.Data;
using UrlShortener.Infrastructure.Repositories;

var builder = WebApplication.CreateSlimBuilder(args);
// Add services
builder.Services.AddDbContextPool<UrlShortenerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddStackExchangeRedisCache(options =>
    options.Configuration = builder.Configuration.GetConnectionString("Redis"));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateShortUrlHandler).Assembly));
builder.Services.AddScoped<IUrlRepository, UrlRepository>();
builder.Services.AddScoped<ICodeGenerationService, Base62CodeGenerationService>();
builder.Services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<UrlShortenerDbContext>());
// Add OpenAPI support for .NET 9
builder.Services.AddOpenApi();
var app = builder.Build();
// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "URL Shortener API"));
}
// API endpoints with route grouping
var urlsGroup = app.MapGroup("/api/urls")
    .WithTags("URL Management")
    .WithOpenApi();
// Create short URL endpoint
urlsGroup.MapPost("/", async (
    CreateShortUrlRequest request,
    ISender mediator,
    CancellationToken cancellationToken) =>
{
    var command = new CreateShortUrlCommand(
        request.OriginalUrl,
        request.CustomCode,
        request.ExpiresAt,
        request.CreatedBy);
    var response = await mediator.Send(command, cancellationToken);
    return Results.Created($"/api/urls/{response.ShortCode}", response);
})
.WithName("CreateShortUrl")
.WithSummary("Create a new short URL")
.WithDescription("Creates a shortened version of the provided URL")
.Produces<CreateShortUrlResponse>(StatusCodes.Status201Created)
.ProducesValidationProblem();
// Redirect endpoint - optimized for performance
app.MapGet("/{code}", async (
    string code,
    ISender mediator,
    HttpContext context,
    CancellationToken cancellationToken) =>
{
    var query = new GetUrlByCodeQuery(code);
    var result = await mediator.Send(query, cancellationToken);
    if (result == null)
        return Results.NotFound("Short URL not found");
    if (result.IsExpired())
        return Results.Gone("Short URL has expired");
    // Track analytics asynchronously
    _ = Task.Run(async () =>
    {
        var analyticsCommand = new RecordClickCommand(
            result.Id,
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            context.Request.Headers.UserAgent.FirstOrDefault(),
            context.Request.Headers.Referer.FirstOrDefault());
            
        await mediator.Send(analyticsCommand, CancellationToken.None);
    }, cancellationToken);
    return Results.Redirect(result.OriginalUrl, permanent: false);
})
.WithName("RedirectToOriginalUrl")
.WithSummary("Redirect to original URL")
.WithDescription("Redirects to the original URL associated with the short code")
.ExcludeFromDescription(); // Don't show in OpenAPI docs
app.Run();