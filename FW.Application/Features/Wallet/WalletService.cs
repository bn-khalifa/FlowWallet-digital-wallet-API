using FW.Domain;

namespace FW.Application;

public interface IWalletService
{
    Task<ApiResponse<WalletDto>> GetMyWalletAsync(Guid userId, CancellationToken ct = default);
    Task<ApiResponse<WalletDto>> UpdateDailyLimitAsync(Guid userId, UpdateDailyLimitRequest request, CancellationToken ct = default);
}

// Implementation 

public class WalletService(IWalletRepository walletRepo, IUnitOfWork uow) : IWalletService
{
    public async Task<ApiResponse<WalletDto>> GetMyWalletAsync(Guid userId, CancellationToken ct = default)
    {
        var wallet = await walletRepo.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException("Wallet", userId);

        return ApiResponse<WalletDto>.Ok(ToDto(wallet));
    }

    public async Task<ApiResponse<WalletDto>> UpdateDailyLimitAsync(Guid userId, UpdateDailyLimitRequest request, CancellationToken ct = default)
    {
        var wallet = await walletRepo.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException("Wallet", userId);

        wallet.UpdateDailyLimit(request.NewLimit);
        await uow.SaveChangesAsync(ct);

        return ApiResponse<WalletDto>.Ok(ToDto(wallet), "Daily limit updated.");
    }

    private static WalletDto ToDto(Wallet w) => new(
        w.Id,
        w.Balance,
        //w.DailyTransferLimit,
        w.DailyTransferUsed,
        Wallet.DailyTransferLimit - w.DailyTransferUsed,
        w.IsActive,
        w.CreatedAt);
}

