using System.ComponentModel.DataAnnotations;

namespace UrlShortener.Api.Models;

public record CreateShortUrlRequest(
    [Required] string OriginalUrl,
    string? CustomCode = null,
    DateTime? ExpiresAt = null,
    string? CreatedBy = null);
public record UrlAnalyticsResponse(
    string ShortCode,
    string OriginalUrl,
    int TotalClicks,
    DateTime CreatedAt,
    IEnumerable<DailyClickCount> DailyClicks);
public record DailyClickCount(DateOnly Date, int Count);