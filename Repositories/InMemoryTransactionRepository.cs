using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Repositories;

/// <summary>
/// In-memory implementation of transaction repository.
/// </summary>
public class InMemoryTransactionRepository : ITransactionRepository
{
    private readonly List<Transaction> _transactions = new();
    private int _nextId = 1;

    public Transaction Add(Transaction transaction)
    {
        transaction.Id = _nextId++;
        _transactions.Add(transaction);
        return transaction;
    }

    public bool Update(Transaction transaction)
    {
        var existing = _transactions.FirstOrDefault(t => t.Id == transaction.Id);
        if (existing == null)
            return false;

        existing.Date = transaction.Date;
        existing.Amount = transaction.Amount;
        existing.Type = transaction.Type;
        existing.ExpenseCategory = transaction.ExpenseCategory;
        existing.IncomeCategory = transaction.IncomeCategory;
        existing.Description = transaction.Description;
        return true;
    }

    public bool Delete(int id)
    {
        var transaction = _transactions.FirstOrDefault(t => t.Id == id);
        if (transaction == null)
            return false;

        _transactions.Remove(transaction);
        return true;
    }

    public Transaction? GetById(int id)
    {
        return _transactions.FirstOrDefault(t => t.Id == id);
    }

    public IReadOnlyList<Transaction> GetAll()
    {
        return _transactions.AsReadOnly();
    }

    public void Clear()
    {
        _transactions.Clear();
        _nextId = 1;
    }

    /// <summary>
    /// Sets the next ID (useful when loading from external source).
    /// </summary>
    public void SetNextId(int nextId)
    {
        _nextId = nextId;
    }
}
