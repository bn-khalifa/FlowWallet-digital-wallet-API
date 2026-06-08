namespace FW.Domain;

public enum TransactionType
{
    Deposit = 1,
    Withdrawal = 2,
    Transfer = 3
}

public enum TransactionStatus
{
    Pending = 1,
    Completed = 2,
    Failed = 3
}
