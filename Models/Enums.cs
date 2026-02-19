namespace PersonalFinanceTracker.Models;

/// <summary>
/// Indicates whether a transaction is income or expense.
/// </summary>
public enum TransactionType
{
    Income,
    Expense
}

/// <summary>
/// Categories available for expense transactions.
/// </summary>
public enum ExpenseCategory
{
    Food,
    Rent,
    Transport,
    Utilities,
    Entertainment,
    Other
}

/// <summary>
/// Categories available for income transactions.
/// </summary>
public enum IncomeCategory
{
    Salary,
    Business,
    Rental,
    Investment,
    Other
}
