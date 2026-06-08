using FW.Domain;
using Microsoft.EntityFrameworkCore;

namespace FW.Infrastructure;

public class TransactionRepository(AppDbContext db) : ITransactionRepository
{
    public Task<Transaction?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.Transactions.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<(IEnumerable<Transaction> Items, int TotalCount)> GetByWalletIdAsync(
        Guid walletId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.Transactions
            .Where(t => t.SenderWalletId == walletId || t.ReceiverWalletId == walletId)
            .OrderByDescending(t => t.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task AddAsync(Transaction transaction, CancellationToken ct = default)
        => await db.Transactions.AddAsync(transaction, ct);
}