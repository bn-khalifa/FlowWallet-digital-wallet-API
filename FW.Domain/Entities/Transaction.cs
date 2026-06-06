namespace FW.Domain
{
    public class Transaction
    {
        public Guid Id { get; private set; }
        public Guid SenderWalletId { get; private set; }
        public Guid? ReceiverWalletId { get; private set; }
        public decimal Amount { get; private set; }
        public TransactionType Type { get; private set; }
        public TransactionStatus Status { get; private set; }
        public string? Note { get; private set; }
        public string? FailureReason { get; private set; }
        public DateTime CreatedAt { get; private set; }

        // Nav Props
        public Wallet SenderWallet { get; private set; } = default!;
        public Wallet? ReceiverWallet { get; private set; }

        private Transaction() { }

        // Transaction related methods
        public static Transaction Create(
        Guid senderWalletId,
        decimal amount,
        TransactionType type,
        Guid? receiverWalletId = null,
        string? note = null)
        {
            return new Transaction
            {
                Id = Guid.NewGuid(),
                SenderWalletId = senderWalletId,
                ReceiverWalletId = receiverWalletId,
                Amount = amount,
                Type = type,
                Status = TransactionStatus.Pending,
                Note = note,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void Complete() => Status = TransactionStatus.Completed;

        public void Fail(string reason)
        {
            Status = TransactionStatus.Failed;
            FailureReason = reason;
        }
    }
}
