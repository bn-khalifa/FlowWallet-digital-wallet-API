using FW.Domain;

namespace FW.Application;

public interface ITransactionService
{
    Task<ApiResponse<TransactionDto>> DepositAsync(Guid userId, DepositRequest request, CancellationToken ct = default);
    Task<ApiResponse<TransactionDto>> WithdrawAsync(Guid userId, WithdrawRequest request, CancellationToken ct = default);
    Task<ApiResponse<TransactionDto>> TransferAsync(Guid userId, TransferRequest request, CancellationToken ct = default);
    Task<ApiResponse<PagedResult<TransactionDto>>> GetHistoryAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
    Task<ApiResponse<TransactionDto>> GetByIdAsync(Guid userId, Guid transactionId, CancellationToken ct = default);
}

// Implementation
public class TransactionService(
    IWalletRepository walletRepo,
    ITransactionRepository transactionRepo,
    IUnitOfWork uow) : ITransactionService
{
    public async Task<ApiResponse<TransactionDto>> DepositAsync(Guid userId, DepositRequest request, CancellationToken ct = default)
    {
        var wallet = await GetWalletOrThrowAsync(userId, ct);
        var tx = Transaction.Create(wallet.Id, request.Amount, TransactionType.Deposit, note: request.Note);

        await uow.ExecuteInTransactionAsync(async () =>
        {
            wallet.Deposit(request.Amount);
            tx.Complete();
            await transactionRepo.AddAsync(tx, ct);
            await uow.SaveChangesAsync(ct);
        }, ct);

        return ApiResponse<TransactionDto>.Ok(ToDto(tx), "Deposit successful.");
    }

    public async Task<ApiResponse<TransactionDto>> WithdrawAsync(Guid userId, WithdrawRequest request, CancellationToken ct = default)
    {
        var wallet = await GetWalletOrThrowAsync(userId, ct);
        var tx = Transaction.Create(wallet.Id, request.Amount, TransactionType.Withdrawal, note: request.Note);

        await uow.ExecuteInTransactionAsync(async () =>
        {
            wallet.Withdraw(request.Amount);
            tx.Complete();
            await transactionRepo.AddAsync(tx, ct);
            await uow.SaveChangesAsync(ct);
        }, ct);

        return ApiResponse<TransactionDto>.Ok(ToDto(tx), "Withdrawal successful.");
    }

    public async Task<ApiResponse<TransactionDto>> TransferAsync(Guid userId, TransferRequest request, CancellationToken ct = default)
    {
        var senderWallet = await GetWalletOrThrowAsync(userId, ct);

        if (senderWallet.Id == request.ReceiverWalletId)
            throw new DomainException("Cannot transfer to your own wallet.");

        var receiverWallet = await walletRepo.GetByIdAsync(request.ReceiverWalletId, ct)
            ?? throw new NotFoundException("Receiver wallet", request.ReceiverWalletId);

        var tx = Transaction.Create(
            senderWallet.Id, request.Amount, TransactionType.Transfer,
            receiverWalletId: receiverWallet.Id, note: request.Note);

        await uow.ExecuteInTransactionAsync(async () =>
        {
            senderWallet.DebitTransfer(request.Amount);
            receiverWallet.CreditTransfer(request.Amount);
            tx.Complete();
            await transactionRepo.AddAsync(tx, ct);
            await uow.SaveChangesAsync(ct);
        }, ct);

        return ApiResponse<TransactionDto>.Ok(ToDto(tx), "Transfer successful.");
    }

    public async Task<ApiResponse<PagedResult<TransactionDto>>> GetHistoryAsync(Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        var wallet = await GetWalletOrThrowAsync(userId, ct);
        var (items, total) = await transactionRepo.GetByWalletIdAsync(wallet.Id, page, pageSize, ct);

        return ApiResponse<PagedResult<TransactionDto>>.Ok(new PagedResult<TransactionDto>
        {
            Items = items.Select(ToDto),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<ApiResponse<TransactionDto>> GetByIdAsync(Guid userId, Guid transactionId, CancellationToken ct = default)
    {
        var wallet = await GetWalletOrThrowAsync(userId, ct);
        var tx = await transactionRepo.GetByIdAsync(transactionId, ct)
            ?? throw new NotFoundException("Transaction", transactionId);

        if (tx.SenderWalletId != wallet.Id && tx.ReceiverWalletId != wallet.Id)
            throw new UnauthorizedException("Transaction does not belong to your wallet.");

        return ApiResponse<TransactionDto>.Ok(ToDto(tx));
    }

    private async Task<Wallet> GetWalletOrThrowAsync(Guid userId, CancellationToken ct)
        => await walletRepo.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException("Wallet", userId);

    private static TransactionDto ToDto(Transaction t) => new(
        t.Id, t.Amount,
        t.Type.ToString(), t.Status.ToString(),
        t.SenderWalletId, t.ReceiverWalletId,
        t.Note, t.FailureReason, t.CreatedAt);
}
