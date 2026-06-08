namespace FW.Application;

public record DepositRequest(decimal Amount, string? Note);
public record WithdrawRequest(decimal Amount, string? Note);
public record TransferRequest(Guid ReceiverWalletId, decimal Amount, string? Note);

public record TransactionDto(
    Guid Id,
    decimal Amount,
    string Type,
    string Status,
    Guid SenderWalletId,
    Guid? ReceiverWalletId,
    string? Note,
    string? FailureReason,
    DateTime CreatedAt);