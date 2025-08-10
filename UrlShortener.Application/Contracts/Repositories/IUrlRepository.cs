using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.Contracts.Repositories;

public interface IUrlRepository
{
    Task<ShortenedUrl?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task AddAsync(ShortenedUrl url, CancellationToken cancellationToken = default);

    Task<ShortenedUrl?> GetByOriginalUrlAsync(string originalUrl,
        CancellationToken cancellationToken = default);
}