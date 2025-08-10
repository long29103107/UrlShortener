namespace UrlShortener.Domain.Entities;

public class ShortenedUrl
{
    public Guid Id { get; private set; }
    public string OriginalUrl { get; private set; } = string.Empty;
    public string ShortCode { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public int ClickCount { get; private set; }
    public bool IsActive { get; private set; } = true;
    public string? CreatedBy { get; private set; }
    private ShortenedUrl() { } // EF Core constructor
    public static ShortenedUrl Create(string originalUrl, string shortCode, string? createdBy = null, DateTime? expiresAt = null)
    {
        if (string.IsNullOrWhiteSpace(originalUrl))
            throw new ArgumentException("Original URL cannot be empty", nameof(originalUrl));
        
        if (string.IsNullOrWhiteSpace(shortCode))
            throw new ArgumentException("Short code cannot be empty", nameof(shortCode));
        return new ShortenedUrl
        {
            Id = Guid.NewGuid(),
            OriginalUrl = originalUrl,
            ShortCode = shortCode,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            CreatedBy = createdBy
        };
    }
    public void IncrementClickCount()
    {
        ClickCount++;
    }
    public void Deactivate()
    {
        IsActive = false;
    }
    public bool IsExpired()
    {
        return ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;
    }
}