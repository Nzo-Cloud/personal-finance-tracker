using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Repositories;
using PersonalFinanceTracker.Services;

namespace PersonalFinanceTracker.Tests;

public class FinanceServiceTests
{
    private readonly InMemoryTransactionRepository _repository;
    private readonly FinanceService _service;

    public FinanceServiceTests()
    {
        _repository = new InMemoryTransactionRepository();
        _service = new FinanceService(_repository);
    }

    [Fact]
    public void AddExpense_ShouldCreateTransactionWithCorrectProperties()
    {
        // Arrange
        var date = new DateTime(2024, 1, 15);
        var amount = 50.00m;
        var category = ExpenseCategory.Food;
        var description = "Groceries";

        // Act
        var transaction = _service.AddExpense(date, amount, category, description);

        // Assert
        Assert.NotEqual(0, transaction.Id);
        Assert.Equal(date, transaction.Date);
        Assert.Equal(amount, transaction.Amount);
        Assert.Equal(TransactionType.Expense, transaction.Type);
        Assert.Equal(category, transaction.ExpenseCategory);
        Assert.Null(transaction.IncomeCategory);
        Assert.Equal(description, transaction.Description);
    }

    [Fact]
    public void AddIncome_ShouldCreateTransactionWithCorrectProperties()
    {
        // Arrange
        var date = new DateTime(2024, 1, 1);
        var amount = 5000.00m;
        var category = IncomeCategory.Salary;
        var description = "Monthly salary";

        // Act
        var transaction = _service.AddIncome(date, amount, category, description);

        // Assert
        Assert.NotEqual(0, transaction.Id);
        Assert.Equal(date, transaction.Date);
        Assert.Equal(amount, transaction.Amount);
        Assert.Equal(TransactionType.Income, transaction.Type);
        Assert.Equal(category, transaction.IncomeCategory);
        Assert.Null(transaction.ExpenseCategory);
        Assert.Equal(description, transaction.Description);
    }

    [Fact]
    public void GetAllTransactions_ShouldReturnAllAddedTransactions()
    {
        // Arrange
        _service.AddExpense(DateTime.Now, 100m, ExpenseCategory.Food, "Test expense 1");
        _service.AddExpense(DateTime.Now, 200m, ExpenseCategory.Transport, "Test expense 2");
        _service.AddIncome(DateTime.Now, 1000m, IncomeCategory.Salary, "Test income");

        // Act
        var transactions = _service.GetAllTransactions();

        // Assert
        Assert.Equal(3, transactions.Count);
    }

    [Fact]
    public void GetTransaction_ShouldReturnCorrectTransaction()
    {
        // Arrange
        var added = _service.AddExpense(DateTime.Now, 100m, ExpenseCategory.Food, "Test");

        // Act
        var retrieved = _service.GetTransaction(added.Id);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(added.Id, retrieved.Id);
        Assert.Equal(added.Description, retrieved.Description);
    }

    [Fact]
    public void GetTransaction_WithInvalidId_ShouldReturnNull()
    {
        // Act
        var retrieved = _service.GetTransaction(999);

        // Assert
        Assert.Null(retrieved);
    }

    [Fact]
    public void DeleteTransaction_ShouldRemoveTransaction()
    {
        // Arrange
        var added = _service.AddExpense(DateTime.Now, 100m, ExpenseCategory.Food, "Test");

        // Act
        var result = _service.DeleteTransaction(added.Id);

        // Assert
        Assert.True(result);
        Assert.Null(_service.GetTransaction(added.Id));
        Assert.Empty(_service.GetAllTransactions());
    }

    [Fact]
    public void DeleteTransaction_WithInvalidId_ShouldReturnFalse()
    {
        // Act
        var result = _service.DeleteTransaction(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void UpdateTransaction_ShouldModifyExistingTransaction()
    {
        // Arrange
        var added = _service.AddExpense(DateTime.Now, 100m, ExpenseCategory.Food, "Original");
        added.Amount = 200m;
        added.Description = "Updated";

        // Act
        var result = _service.UpdateTransaction(added);

        // Assert
        Assert.True(result);
        var retrieved = _service.GetTransaction(added.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(200m, retrieved.Amount);
        Assert.Equal("Updated", retrieved.Description);
    }

    [Fact]
    public void GetSummary_ShouldCalculateCorrectTotals()
    {
        // Arrange
        _service.AddIncome(DateTime.Now, 5000m, IncomeCategory.Salary, "Salary");
        _service.AddIncome(DateTime.Now, 500m, IncomeCategory.Investment, "Dividends");
        _service.AddExpense(DateTime.Now, 1000m, ExpenseCategory.Rent, "Rent");
        _service.AddExpense(DateTime.Now, 200m, ExpenseCategory.Food, "Groceries");
        _service.AddExpense(DateTime.Now, 100m, ExpenseCategory.Food, "Restaurant");

        // Act
        var summary = _service.GetSummary();

        // Assert
        Assert.Equal(5500m, summary.TotalIncome);
        Assert.Equal(1300m, summary.TotalExpense);
        Assert.Equal(4200m, summary.Balance);
    }

    [Fact]
    public void GetSummary_ShouldGroupByCategory()
    {
        // Arrange
        _service.AddIncome(DateTime.Now, 5000m, IncomeCategory.Salary, "Salary");
        _service.AddIncome(DateTime.Now, 500m, IncomeCategory.Salary, "Bonus");
        _service.AddExpense(DateTime.Now, 200m, ExpenseCategory.Food, "Groceries");
        _service.AddExpense(DateTime.Now, 100m, ExpenseCategory.Food, "Restaurant");
        _service.AddExpense(DateTime.Now, 50m, ExpenseCategory.Transport, "Bus");

        // Act
        var summary = _service.GetSummary();

        // Assert
        Assert.Equal(5500m, summary.IncomeByCategory[IncomeCategory.Salary]);
        Assert.Equal(300m, summary.ExpenseByCategory[ExpenseCategory.Food]);
        Assert.Equal(50m, summary.ExpenseByCategory[ExpenseCategory.Transport]);
    }

    [Fact]
    public void GetSummary_WithNoTransactions_ShouldReturnZeros()
    {
        // Act
        var summary = _service.GetSummary();

        // Assert
        Assert.Equal(0m, summary.TotalIncome);
        Assert.Equal(0m, summary.TotalExpense);
        Assert.Equal(0m, summary.Balance);
        Assert.Empty(summary.IncomeByCategory);
        Assert.Empty(summary.ExpenseByCategory);
    }

    [Fact]
    public void Transaction_CategoryName_ShouldReturnCorrectValue()
    {
        // Arrange
        var expense = _service.AddExpense(DateTime.Now, 100m, ExpenseCategory.Entertainment, "Movie");
        var income = _service.AddIncome(DateTime.Now, 1000m, IncomeCategory.Business, "Consulting");

        // Assert
        Assert.Equal("Entertainment", expense.CategoryName);
        Assert.Equal("Business", income.CategoryName);
    }
}
