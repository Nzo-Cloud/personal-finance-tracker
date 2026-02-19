namespace PersonalFinanceTracker.Models;

/// <summary>
/// Result object containing financial summary data.
/// </summary>
public class FinanceSummary
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal Balance => TotalIncome - TotalExpense;
    
    /// <summary>
    /// Totals grouped by expense category.
    /// </summary>
    public Dictionary<ExpenseCategory, decimal> ExpenseByCategory { get; set; } = new();
    
    /// <summary>
    /// Totals grouped by income category.
    /// </summary>
    public Dictionary<IncomeCategory, decimal> IncomeByCategory { get; set; } = new();
}
