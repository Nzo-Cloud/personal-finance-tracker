namespace PersonalFinanceTracker.Models;

/// <summary>
/// Represents a single financial transaction with strongly-typed categories.
/// </summary>
public class Transaction
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    
    /// <summary>
    /// Category for expense transactions. Null if this is an income transaction.
    /// </summary>
    public ExpenseCategory? ExpenseCategory { get; set; }
    
    /// <summary>
    /// Category for income transactions. Null if this is an expense transaction.
    /// </summary>
    public IncomeCategory? IncomeCategory { get; set; }
    
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets the category name as a string, regardless of transaction type.
    /// </summary>
    public string CategoryName => Type == TransactionType.Expense 
        ? ExpenseCategory?.ToString() ?? "Unknown"
        : IncomeCategory?.ToString() ?? "Unknown";

    public Transaction() { }

    public Transaction(int id, DateTime date, decimal amount, TransactionType type, string description)
    {
        Id = id;
        Date = date;
        Amount = amount;
        Type = type;
        Description = description;
    }

    /// <summary>
    /// Creates an expense transaction.
    /// </summary>
    public static Transaction CreateExpense(int id, DateTime date, decimal amount, ExpenseCategory category, string description)
    {
        return new Transaction(id, date, amount, TransactionType.Expense, description)
        {
            ExpenseCategory = category
        };
    }

    /// <summary>
    /// Creates an income transaction.
    /// </summary>
    public static Transaction CreateIncome(int id, DateTime date, decimal amount, IncomeCategory category, string description)
    {
        return new Transaction(id, date, amount, TransactionType.Income, description)
        {
            IncomeCategory = category
        };
    }
}
