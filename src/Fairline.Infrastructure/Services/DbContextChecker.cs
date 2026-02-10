using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Fairline.Abstractions.Interfaces;
using Fairline.Infrastructure.Persistence;

namespace Fairline.Infrastructure.Services;

public sealed class DbContextChecker(IngestDbContext db, ILogger<DbContextChecker> logger) : IDbContextChecker
{
    public async Task<bool> CanConnectAsync(CancellationToken ct = default)
    {
        try
        {
            return await db.Database.CanConnectAsync(ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Database connectivity check failed");
            return false;
        }
    }
}
