using FW.Domain;
using Microsoft.EntityFrameworkCore;

namespace FW.Infrastructure;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => db.Users.FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), ct);

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
        => db.Users.AnyAsync(u => u.Email == email.ToLowerInvariant(), ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
        => await db.Users.AddAsync(user, ct);
}