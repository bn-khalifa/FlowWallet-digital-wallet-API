namespace FW.Application;

public record WalletDto(
    Guid Id,
    decimal Balance,
    //decimal DailyTransferLimit,
    decimal DailyTransferUsed,
    decimal DailyTransferRemaining,
    bool IsActive,
    DateTime CreatedAt);

public record UpdateDailyLimitRequest(decimal NewLimit);
