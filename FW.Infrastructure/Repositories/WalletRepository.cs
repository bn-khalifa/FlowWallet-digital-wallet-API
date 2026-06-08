using FW.Domain;
using Microsoft.EntityFrameworkCore;

namespace FW.Infrastructure;

public class WalletRepository(AppDbContext db) : IWalletRepository
{
    public Task<Wallet?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.Wallets.FirstOrDefaultAsync(w => w.Id == id, ct);

    public Task<Wallet?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => db.Wallets.FirstOrDefaultAsync(w => w.UserId == userId, ct);

    public async Task AddAsync(Wallet wallet, CancellationToken ct = default)
        => await db.Wallets.AddAsync(wallet, ct);
}