using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Repositories;

/// <summary>
/// Interface for transaction data access.
/// </summary>
public interface ITransactionRepository
{
    /// <summary>
    /// Adds a new transaction and returns it with the assigned ID.
    /// </summary>
    Transaction Add(Transaction transaction);
    
    /// <summary>
    /// Updates an existing transaction.
    /// </summary>
    /// <returns>True if the transaction was found and updated.</returns>
    bool Update(Transaction transaction);
    
    /// <summary>
    /// Deletes a transaction by ID.
    /// </summary>
    /// <returns>True if the transaction was found and deleted.</returns>
    bool Delete(int id);
    
    /// <summary>
    /// Gets a transaction by ID.
    /// </summary>
    /// <returns>The transaction, or null if not found.</returns>
    Transaction? GetById(int id);
    
    /// <summary>
    /// Gets all transactions.
    /// </summary>
    IReadOnlyList<Transaction> GetAll();
    
    /// <summary>
    /// Clears all transactions.
    /// </summary>
    void Clear();
}
