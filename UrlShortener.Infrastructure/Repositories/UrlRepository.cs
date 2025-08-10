using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using UrlShortener.Application.Contracts.Repositories;
using UrlShortener.Domain.Entities;
using UrlShortener.Infrastructure.Data;

namespace UrlShortener.Infrastructure.Repositories;

public class UrlRepository(UrlShortenerDbContext context, IDistributedCache cache, ILogger<UrlRepository> logger)
    : IUrlRepository
{
    // Compiled queries for optimal performance
    private static readonly Func<UrlShortenerDbContext, string, CancellationToken, Task<ShortenedUrl?>> 
        _getByCodeQuery = EF.CompileAsyncQuery(
            (UrlShortenerDbContext ctx, string code, CancellationToken ct) =>
                ctx.ShortenedUrls.FirstOrDefault(u => u.ShortCode == code && u.IsActive));
    private static readonly Func<UrlShortenerDbContext, string, CancellationToken, Task<bool>> 
        _existsByCodeQuery = EF.CompileAsyncQuery(
            (UrlShortenerDbContext ctx, string code, CancellationToken ct) =>
                ctx.ShortenedUrls.Any(u => u.ShortCode == code));

    public async Task<ShortenedUrl?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        // Try cache first
        var cacheKey = $"url:{code}";
        var cachedUrl = await cache.GetStringAsync(cacheKey, cancellationToken);
        
        if (!string.IsNullOrEmpty(cachedUrl))
        {
            try
            {
                return JsonSerializer.Deserialize<ShortenedUrl>(cachedUrl);
            }
            catch (JsonException ex)
            {
                logger.LogWarning(ex, "Failed to deserialize cached URL for code {Code}", code);
            }
        }
        // Fallback to database with compiled query
        var url = await _getByCodeQuery(context, code, cancellationToken);
        
        if (url != null)
        {
            // Cache for 1 hour
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            };
            
            await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(url), cacheOptions, cancellationToken);
        }
        
        return url;
    }
    public async Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _existsByCodeQuery(context, code, cancellationToken);
    }
    public async Task AddAsync(ShortenedUrl url, CancellationToken cancellationToken = default)
    {
        await context.ShortenedUrls.AddAsync(url, cancellationToken);
    }
    public async Task<ShortenedUrl?> GetByOriginalUrlAsync(string originalUrl, CancellationToken cancellationToken = default)
    {
        return await context.ShortenedUrls
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.OriginalUrl == originalUrl && u.IsActive, cancellationToken);
    }
}