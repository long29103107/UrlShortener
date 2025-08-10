using Microsoft.EntityFrameworkCore;
using UrlShortener.Application.Contracts.Repositories;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Infrastructure.Data;

public class UrlShortenerDbContext: DbContext, IUnitOfWork

{
    public UrlShortenerDbContext(DbContextOptions<UrlShortenerDbContext> options) : base(options) { }
    public DbSet<ShortenedUrl> ShortenedUrls => Set<ShortenedUrl>();
    public DbSet<ClickAnalytics> ClickAnalytics => Set<ClickAnalytics>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ShortenedUrl configuration
        modelBuilder.Entity<ShortenedUrl>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.OriginalUrl)
                .IsRequired()
                .HasMaxLength(2048);
                
            entity.Property(e => e.ShortCode)
                .IsRequired()
                .HasMaxLength(10);
                
            entity.HasIndex(e => e.ShortCode)
                .IsUnique()
                .HasDatabaseName("IX_ShortenedUrls_ShortCode");
                
            entity.HasIndex(e => e.OriginalUrl)
                .HasDatabaseName("IX_ShortenedUrls_OriginalUrl");
                
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(256);
        });
        // ClickAnalytics configuration
        modelBuilder.Entity<ClickAnalytics>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.IpAddress)
                .IsRequired()
                .HasMaxLength(45); // IPv6 support
                
            entity.Property(e => e.UserAgent)
                .HasMaxLength(512);
                
            entity.Property(e => e.Referrer)
                .HasMaxLength(2048);
                
            entity.HasIndex(e => e.ShortenedUrlId)
                .HasDatabaseName("IX_ClickAnalytics_ShortenedUrlId");
                
            entity.HasIndex(e => e.ClickedAt)
                .HasDatabaseName("IX_ClickAnalytics_ClickedAt");
        });
        base.OnModelCreating(modelBuilder);
    }
    
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}