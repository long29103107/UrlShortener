using MediatR;

namespace UrlShortener.Application.Features.UrlShortening.Commands;

public record CreateShortUrlCommand(
    string OriginalUrl,
    string? CustomCode = null,
    DateTime? ExpiresAt = null,
    string? CreatedBy = null
) : IRequest<CreateShortUrlResponse>;
public record CreateShortUrlResponse(
    string ShortCode,
    string ShortUrl,
    string OriginalUrl,
    DateTime CreatedAt,
    DateTime? ExpiresAt
);