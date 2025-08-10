namespace UrlShortener.Domain.Entities;

public class ClickAnalytics
{
    public Guid Id { get; private set; }
    public Guid ShortenedUrlId { get; private set; }
    public string IpAddress { get; private set; } = string.Empty;
    public string? UserAgent { get; private set; }
    public string? Referrer { get; private set; }
    public DateTime ClickedAt { get; private set; }
    public string? Country { get; private set; }
    public string? City { get; private set; }
    private ClickAnalytics() { }
    public static ClickAnalytics Create(Guid shortenedUrlId, string ipAddress, string? userAgent = null, string? referrer = null)
    {
        return new ClickAnalytics
        {
            Id = Guid.NewGuid(),
            ShortenedUrlId = shortenedUrlId,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Referrer = referrer,
            ClickedAt = DateTime.UtcNow
        };
    }
}