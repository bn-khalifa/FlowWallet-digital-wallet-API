using FW.Domain;
using Microsoft.EntityFrameworkCore;

namespace FW.Infrastructure;

public class RefreshTokenRepository(AppDbContext db) : IRefreshTokenRepository
{
    public Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default)
        => db.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == token, ct);

    public async Task AddAsync(RefreshToken token, CancellationToken ct = default)
        => await db.RefreshTokens.AddAsync(token, ct);

    public Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default)
        => db.RefreshTokens
            .Where(r => r.UserId == userId && !r.IsRevoked)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.IsRevoked, true), ct);
}