using System.Transactions;

namespace FW.Domain
{
    public class Wallet
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public decimal Balance { get; private set; }
        public static decimal DailyTransferLimit { get; private set; } = 10_000m;
        public decimal DailyTransferUsed { get; private set; }
        public DateTime DailyLimitResetDate { get; private set; }
        public bool IsActive { get; private set; } = true;
        public DateTime CreatedAt { get; private set; }

        // Nav Props
        public User User { get; private set; } = default!;
        public ICollection<Transaction> OutgoingTransactions { get; private set; } = [];
        public ICollection<Transaction> IncomingTransactions { get; private set; } = [];

        private Wallet() { }

        // Wallet related methods
        public static Wallet Create(Guid userId)
        {
            return new Wallet
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Balance = 0,
                DailyTransferUsed = 0,
                DailyLimitResetDate = DateTime.UtcNow.Date,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void Deposit(decimal amount)
        {
            try
            {
                IsValid(amount, false);
            }
            catch (DomainException ex)
            {
                throw new DomainException($"Deposit failed: {ex.Message}");
            }

            Balance += amount;
        }

        public void Withdraw(decimal amount)
        {
            try
            {
                IsValid(amount, true);
            }
            catch (DomainException ex)
            {
                throw new DomainException($"Deposit failed: {ex.Message}");
            }

            Balance -= amount;
        }

        public void DebitTransfer(decimal amount)
        {
            try
            {
                IsValid(amount, true);
            }
            catch (DomainException ex)
            {
                throw new DomainException($"Deposit failed: {ex.Message}");
            }

            ResetDailyLimitIfNeeded();

            if (DailyTransferUsed + amount > DailyTransferLimit)
                throw new DomainException($"Daily transfer limit of {DailyTransferLimit:C} exceeded.");

            Balance -= amount;
            DailyTransferUsed += amount;
        }

        public void CreditTransfer(decimal amount)
        {
            if (!IsActive) throw new DomainException("Receiver wallet is inactive.");
            Balance += amount;
        }

        public void UpdateDailyLimit(decimal newLimit)
        {
            if (newLimit <= 0) throw new DomainException("Daily limit must be positive.");
            DailyTransferLimit = newLimit;
        }

        public void IsValid(decimal amount, bool withdraw)
        {
            if (!IsActive) throw new DomainException("Wallet is inactive.");

            if (amount < 0) throw new DomainException("Amount must be positive.");

            if(withdraw && amount > Balance)  throw new DomainException("Insufficient balance.");
        }

        private void ResetDailyLimitIfNeeded()
        {
            if (DailyLimitResetDate.Date < DateTime.UtcNow.Date)
            {
                DailyTransferUsed = 0;
                DailyLimitResetDate = DateTime.UtcNow.Date;
            }
        }
    }
}
