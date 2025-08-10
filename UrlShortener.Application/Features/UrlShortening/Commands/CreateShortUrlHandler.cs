using MediatR;
using Microsoft.Extensions.Configuration;
using UrlShortener.Application.Contracts.Repositories;
using UrlShortener.Application.Services;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.Features.UrlShortening.Commands;

public class CreateShortUrlHandler(
    IUrlRepository urlRepository,
    ICodeGenerationService codeGenerator,
    IUnitOfWork unitOfWork,
    IConfiguration configuration)
    : IRequestHandler<CreateShortUrlCommand, CreateShortUrlResponse>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IConfiguration _configuration = configuration;

    public async Task<CreateShortUrlResponse> Handle(CreateShortUrlCommand request, CancellationToken cancellationToken)
    {
        // Validate URL format
        if (!Uri.TryCreate(request.OriginalUrl, UriKind.Absolute, out var uri))
            throw new ArgumentException("Invalid URL format");
        // Check if URL already exists (optional optimization)
        var existingUrl = await urlRepository.GetByOriginalUrlAsync(request.OriginalUrl, cancellationToken);
        if (existingUrl != null && existingUrl.IsActive && !existingUrl.IsExpired())
        {
            return CreateResponse(existingUrl);
        }
        // Generate unique short code
        var shortCode = request.CustomCode ?? await GenerateUniqueCodeAsync(cancellationToken);
        
        // Create domain entity
        var shortenedUrl = ShortenedUrl.Create(
            request.OriginalUrl,
            shortCode,
            request.CreatedBy,
            request.ExpiresAt);
        // Persist to database
        await urlRepository.AddAsync(shortenedUrl, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return CreateResponse(shortenedUrl);
    }
    private async Task<string> GenerateUniqueCodeAsync(CancellationToken cancellationToken)
    {
        const int maxAttempts = 10;
        
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            var code = codeGenerator.GenerateCode();
            
            if (!await urlRepository.ExistsByCodeAsync(code, cancellationToken))
                return code;
        }
        
        throw new InvalidOperationException("Unable to generate unique code after maximum attempts");
    }
    private CreateShortUrlResponse CreateResponse(ShortenedUrl shortenedUrl)
    {
        var baseUrl = _configuration["BaseUrl"] ?? "https://localhost:5001";
        var shortUrl = $"{baseUrl}/{shortenedUrl.ShortCode}";
        return new CreateShortUrlResponse(
            shortenedUrl.ShortCode,
            shortUrl,
            shortenedUrl.OriginalUrl,
            shortenedUrl.CreatedAt,
            shortenedUrl.ExpiresAt);
    }
}