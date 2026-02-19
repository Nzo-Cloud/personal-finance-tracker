using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Repositories;
using PersonalFinanceTracker.Services;

namespace PersonalFinanceTracker;

class Program
{
    private static FinanceService _service = null!;
    private static CsvTransactionRepository _repository = null!;
    private const string DefaultFile = "transactions.csv";

    static void Main()
    {
        _repository = new CsvTransactionRepository();
        _service = new FinanceService(_repository);

        // Attempt to load default data on startup
        var loadResult = _repository.LoadFromFile(DefaultFile);
        if (loadResult.Success)
            Console.WriteLine(loadResult.Message);

        bool running = true;

        while (running)
        {
            Console.WriteLine("\n===== Personal Finance Tracker =====");
            Console.WriteLine("1. Add Transaction");
            Console.WriteLine("2. View All Transactions");
            Console.WriteLine("3. View Summary");
            Console.WriteLine("4. Edit Transaction");
            Console.WriteLine("5. Delete Transaction");
            Console.WriteLine("6. Save to CSV");
            Console.WriteLine("7. Load from CSV");
            Console.WriteLine("8. Exit");
            Console.Write("\nSelect option: ");

            string input = Console.ReadLine() ?? "";

            switch (input)
            {
                case "1":
                    AddTransactionFlow();
                    break;
                case "2":
                    DisplayTransactions();
                    break;
                case "3":
                    DisplaySummary();
                    break;
                case "4":
                    EditTransactionFlow();
                    break;
                case "5":
                    DeleteTransactionFlow();
                    break;
                case "6":
                    SaveToFile();
                    break;
                case "7":
                    LoadFromFile();
                    break;
                case "8":
                    running = false;
                    Console.WriteLine("Exiting...");
                    break;
                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }
    }

    static void DisplayTransactions()
    {
        var transactions = _service.GetAllTransactions();

        if (transactions.Count == 0)
        {
            Console.WriteLine("\nNo transactions found.");
            return;
        }

        Console.WriteLine("\n  ID | Date       | Type    | Category      |     Amount | Description");
        Console.WriteLine("--------------------------------------------------------------------------");

        foreach (var t in transactions)
        {
            Console.WriteLine($"{t.Id,4} | {t.Date:yyyy-MM-dd} | {t.Type,-7} | {t.CategoryName,-13} | {t.Amount,10:C} | {t.Description}");
        }
    }

    static void DisplaySummary()
    {
        var summary = _service.GetSummary();

        Console.WriteLine("\n----- Summary -----");
        Console.WriteLine($"Total Income:  {summary.TotalIncome:C}");
        Console.WriteLine($"Total Expense: {summary.TotalExpense:C}");
        Console.WriteLine($"Balance:       {summary.Balance:C}");

        if (summary.IncomeByCategory.Count > 0)
        {
            Console.WriteLine("\nIncome by Category:");
            foreach (var kvp in summary.IncomeByCategory)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:C}");
        }

        if (summary.ExpenseByCategory.Count > 0)
        {
            Console.WriteLine("\nExpenses by Category:");
            foreach (var kvp in summary.ExpenseByCategory)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:C}");
        }
    }

    static void AddTransactionFlow()
    {
        bool adding = true;

        while (adding)
        {
            // Date
            DateTime date = ReadDate("Enter date (yyyy-MM-dd): ");

            // Amount
            decimal amount = ReadDecimal("Enter amount: ", minValue: 0.01m);

            // Type
            Console.WriteLine("Select type: 1. Income  2. Expense");
            TransactionType type = ReadOption("Select: ", 1, 2) == 1 
                ? TransactionType.Income 
                : TransactionType.Expense;

            // Category & create transaction
            if (type == TransactionType.Expense)
            {
                var category = SelectExpenseCategory();
                string description = ReadNonEmptyString("Enter description: ");
                var t = _service.AddExpense(date, amount, category, description);
                Console.WriteLine($"Expense added with ID {t.Id}.");
            }
            else
            {
                var category = SelectIncomeCategory();
                string description = ReadNonEmptyString("Enter description: ");
                var t = _service.AddIncome(date, amount, category, description);
                Console.WriteLine($"Income added with ID {t.Id}.");
            }

            // Continue?
            Console.Write("Add another transaction? (y/n): ");
            adding = Console.ReadLine()?.Trim().ToLower() == "y";
        }
    }

    static void EditTransactionFlow()
    {
        DisplayTransactions();
        var transactions = _service.GetAllTransactions();
        if (transactions.Count == 0) return;

        Console.Write("\nEnter transaction ID to edit (0 to cancel): ");
        if (!int.TryParse(Console.ReadLine(), out int id) || id == 0)
        {
            Console.WriteLine("Cancelled.");
            return;
        }

        var existing = _service.GetTransaction(id);
        if (existing == null)
        {
            Console.WriteLine($"Transaction with ID {id} not found.");
            return;
        }

        Console.WriteLine($"\nEditing transaction {id}. Press Enter to keep current value.");

        // Date
        Console.Write($"Date ({existing.Date:yyyy-MM-dd}): ");
        string dateInput = Console.ReadLine() ?? "";
        if (!string.IsNullOrWhiteSpace(dateInput) && DateTime.TryParse(dateInput, out DateTime newDate))
            existing.Date = newDate;

        // Amount
        Console.Write($"Amount ({existing.Amount}): ");
        string amountInput = Console.ReadLine() ?? "";
        if (!string.IsNullOrWhiteSpace(amountInput) && decimal.TryParse(amountInput, out decimal newAmount))
            existing.Amount = newAmount;

        // Type
        Console.Write($"Type ({existing.Type}) - 1. Income  2. Expense (Enter to keep): ");
        string typeInput = Console.ReadLine() ?? "";
        if (typeInput == "1")
        {
            existing.Type = TransactionType.Income;
            existing.ExpenseCategory = null;
            existing.IncomeCategory = SelectIncomeCategory();
        }
        else if (typeInput == "2")
        {
            existing.Type = TransactionType.Expense;
            existing.IncomeCategory = null;
            existing.ExpenseCategory = SelectExpenseCategory();
        }
        else
        {
            // Category change without type change
            Console.Write($"Change category? Current: {existing.CategoryName} (y/n): ");
            if (Console.ReadLine()?.Trim().ToLower() == "y")
            {
                if (existing.Type == TransactionType.Expense)
                    existing.ExpenseCategory = SelectExpenseCategory();
                else
                    existing.IncomeCategory = SelectIncomeCategory();
            }
        }

        // Description
        Console.Write($"Description ({existing.Description}): ");
        string descInput = Console.ReadLine() ?? "";
        if (!string.IsNullOrWhiteSpace(descInput))
            existing.Description = descInput;

        if (_service.UpdateTransaction(existing))
            Console.WriteLine("Transaction updated successfully.");
        else
            Console.WriteLine("Failed to update transaction.");
    }

    static void DeleteTransactionFlow()
    {
        DisplayTransactions();
        var transactions = _service.GetAllTransactions();
        if (transactions.Count == 0) return;

        while (true)
        {
            Console.Write("\nEnter transaction ID to delete (0 to cancel): ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid ID.");
                continue;
            }

            if (id == 0)
            {
                Console.WriteLine("Cancelled.");
                return;
            }

            if (_service.DeleteTransaction(id))
            {
                Console.WriteLine("Transaction deleted successfully.");
                return;
            }
            else
            {
                Console.WriteLine($"Transaction with ID {id} not found.");
            }
        }
    }

    static void SaveToFile()
    {
        string fileName = ReadValidFileName("Enter file name to save (e.g., transactions.csv): ");
        var error = _repository.SaveToFile(fileName);
        if (error == null)
            Console.WriteLine($"Transactions saved to {fileName}");
        else
            Console.WriteLine($"Error saving: {error}");
    }

    static void LoadFromFile()
    {
        string fileName = ReadValidFileName("Enter file name to load (e.g., transactions.csv): ");
        var result = _repository.LoadFromFile(fileName);
        Console.WriteLine(result.Message);
    }

    // ========== Input Helpers ==========

    static DateTime ReadDate(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            if (DateTime.TryParse(Console.ReadLine(), out DateTime date))
                return date;
            Console.WriteLine("Invalid date format.");
        }
    }

    static decimal ReadDecimal(string prompt, decimal minValue = decimal.MinValue)
    {
        while (true)
        {
            Console.Write(prompt);
            if (decimal.TryParse(Console.ReadLine(), out decimal value) && value >= minValue)
                return value;
            Console.WriteLine($"Invalid amount. Must be at least {minValue}.");
        }
    }

    static int ReadOption(string prompt, int min, int max)
    {
        while (true)
        {
            Console.Write(prompt);
            if (int.TryParse(Console.ReadLine(), out int value) && value >= min && value <= max)
                return value;
            Console.WriteLine($"Invalid option. Enter {min}-{max}.");
        }
    }

    static string ReadNonEmptyString(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string input = Console.ReadLine() ?? "";
            if (!string.IsNullOrWhiteSpace(input))
                return input;
            Console.WriteLine("Input cannot be empty.");
        }
    }

    static string ReadValidFileName(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string input = (Console.ReadLine() ?? "").Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine($"Using default: {DefaultFile}");
                return DefaultFile;
            }

            if (input.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                Console.WriteLine("File name contains invalid characters.");
                continue;
            }

            return input;
        }
    }

    static ExpenseCategory SelectExpenseCategory()
    {
        Console.WriteLine("Select Expense category:");
        var values = Enum.GetValues<ExpenseCategory>();
        for (int i = 0; i < values.Length; i++)
            Console.WriteLine($"  {i + 1}. {values[i]}");

        int idx = ReadOption("Select: ", 1, values.Length);
        return values[idx - 1];
    }

    static IncomeCategory SelectIncomeCategory()
    {
        Console.WriteLine("Select Income category:");
        var values = Enum.GetValues<IncomeCategory>();
        for (int i = 0; i < values.Length; i++)
            Console.WriteLine($"  {i + 1}. {values[i]}");

        int idx = ReadOption("Select: ", 1, values.Length);
        return values[idx - 1];
    }
}
