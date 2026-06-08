using FW.Domain;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace FW.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // User
        mb.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.FullName).HasMaxLength(100).IsRequired();
            e.Property(x => x.Email).HasMaxLength(150).IsRequired();
            e.Property(x => x.PasswordHash).IsRequired();
            e.Property(x => x.Role).HasMaxLength(20).HasDefaultValue("User");
        });

        // Wallet
        mb.Entity<Wallet>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Balance).HasPrecision(18, 2);
            e.Property(x => Wallet.DailyTransferLimit).HasPrecision(18, 2);
            e.Property(x => x.DailyTransferUsed).HasPrecision(18, 2);
            e.HasOne(x => x.User)
                .WithOne(x => x.Wallet)
                .HasForeignKey<Wallet>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Transaction
        mb.Entity<Transaction>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Amount).HasPrecision(18, 2);
            e.Property(x => x.Note).HasMaxLength(250);
            e.Property(x => x.FailureReason).HasMaxLength(500);
            e.HasOne(x => x.SenderWallet)
                .WithMany(x => x.OutgoingTransactions)
                .HasForeignKey(x => x.SenderWalletId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.ReceiverWallet)
                .WithMany(x => x.IncomingTransactions)
                .HasForeignKey(x => x.ReceiverWalletId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.SenderWalletId);
            e.HasIndex(x => x.CreatedAt);
        });

        // RefreshToken
        mb.Entity<RefreshToken>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Token).IsUnique();
            e.HasOne(x => x.User)
                .WithMany(x => x.RefreshTokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
