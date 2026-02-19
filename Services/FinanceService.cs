using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Repositories;

namespace PersonalFinanceTracker.Services;

/// <summary>
/// Business logic for financial operations. Console-free for testability.
/// </summary>
public class FinanceService
{
    private readonly ITransactionRepository _repository;

    public FinanceService(ITransactionRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Adds an expense transaction.
    /// </summary>
    public Transaction AddExpense(DateTime date, decimal amount, ExpenseCategory category, string description)
    {
        var transaction = new Transaction
        {
            Date = date,
            Amount = amount,
            Type = TransactionType.Expense,
            ExpenseCategory = category,
            Description = description
        };
        return _repository.Add(transaction);
    }

    /// <summary>
    /// Adds an income transaction.
    /// </summary>
    public Transaction AddIncome(DateTime date, decimal amount, IncomeCategory category, string description)
    {
        var transaction = new Transaction
        {
            Date = date,
            Amount = amount,
            Type = TransactionType.Income,
            IncomeCategory = category,
            Description = description
        };
        return _repository.Add(transaction);
    }

    /// <summary>
    /// Gets all transactions.
    /// </summary>
    public IReadOnlyList<Transaction> GetAllTransactions()
    {
        return _repository.GetAll();
    }

    /// <summary>
    /// Gets a transaction by ID.
    /// </summary>
    public Transaction? GetTransaction(int id)
    {
        return _repository.GetById(id);
    }

    /// <summary>
    /// Updates an existing transaction.
    /// </summary>
    public bool UpdateTransaction(Transaction transaction)
    {
        return _repository.Update(transaction);
    }

    /// <summary>
    /// Deletes a transaction by ID.
    /// </summary>
    public bool DeleteTransaction(int id)
    {
        return _repository.Delete(id);
    }

    /// <summary>
    /// Calculates and returns a financial summary.
    /// </summary>
    public FinanceSummary GetSummary()
    {
        var transactions = _repository.GetAll();
        var summary = new FinanceSummary();

        foreach (var t in transactions)
        {
            if (t.Type == TransactionType.Income)
            {
                summary.TotalIncome += t.Amount;
                if (t.IncomeCategory.HasValue)
                {
                    if (!summary.IncomeByCategory.ContainsKey(t.IncomeCategory.Value))
                        summary.IncomeByCategory[t.IncomeCategory.Value] = 0;
                    summary.IncomeByCategory[t.IncomeCategory.Value] += t.Amount;
                }
            }
            else
            {
                summary.TotalExpense += t.Amount;
                if (t.ExpenseCategory.HasValue)
                {
                    if (!summary.ExpenseByCategory.ContainsKey(t.ExpenseCategory.Value))
                        summary.ExpenseByCategory[t.ExpenseCategory.Value] = 0;
                    summary.ExpenseByCategory[t.ExpenseCategory.Value] += t.Amount;
                }
            }
        }

        return summary;
    }
}
