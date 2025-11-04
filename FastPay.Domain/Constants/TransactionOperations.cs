namespace FastPay.Domain.Constants;

public static class TransactionOperations
{
    public const string Credit = "credit";
    public const string Debit = "debit";
    public const string Reserve = "reserve";
    public const string Capture = "capture";
    public const string Reversal = "reversal";
    public const string Transfer = "transfer";

    public static readonly HashSet<string> All = new(StringComparer.OrdinalIgnoreCase)
    {
        Credit, Debit, Reserve, Capture, Reversal, Transfer
    };

    public static bool IsValid(string? op) => !string.IsNullOrWhiteSpace(op) && All.Contains(op);
}

