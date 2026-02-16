using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

//
// ENUMS
// --------------------------------------------------

// Indicates whether a transaction is income or expense
enum TransactionType
{
    Income,
    Expense
}

// Categories available for expense transactions
enum ExpenseCategory
{
    Food,
    Rent,
    Transport,
    Utilities,
    Entertainment,
    Other
}

// Categories available for income transactions
enum IncomeCategory
{
    Salary,
    Business,
    Rental,
    Investment,
    Other
}

//
// DATA MODEL
// --------------------------------------------------

// Represents a single financial transaction
class Transaction
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public string Category { get; set; }
    public string Description { get; set; }

    public Transaction(int id, DateTime date, decimal amount, TransactionType type, string category, string description)
    {
        Id = id;
        Date = date;
        Amount = amount;
        Type = type;
        Category = category;
        Description = description;
    }

    // Displays the transaction in a formatted console-friendly line
    public void Display()
    {
        Console.WriteLine($"{Id,4} | {Date:yyyy-MM-dd} | {Type,-7} | {Category,-13} | {Amount,10:C} | {Description}");
    }
}

//
// BUSINESS LOGIC
// --------------------------------------------------

// Manages transactions and persistence logic
class FinanceManager
{
    private List<Transaction> transactions;
    private int nextId = 1;

    public FinanceManager()
    {
        transactions = new List<Transaction>();
    }

    // Adds a new transaction to memory
    public void AddTransaction(DateTime date, decimal amount, TransactionType type, string category, string description)
    {
        var t = new Transaction(nextId++, date, amount, type, category, description);
        transactions.Add(t);
        Console.WriteLine("Transaction added successfully.");
    }

    // Deletes a transaction by ID
    public bool DeleteTransaction(int id)
    {
        var t = transactions.FirstOrDefault(t => t.Id == id);
        if (t == null)
        {
            Console.WriteLine($"Transaction with ID {id} not found.");
            return false;
        }

        transactions.Remove(t);
        Console.WriteLine("Transaction deleted successfully.");
        return true;
    }

    // Gets a transaction by ID for editing
    public Transaction? GetTransactionById(int id)
    {
        return transactions.FirstOrDefault(t => t.Id == id);
    }

    // Updates a transaction's fields
    public void UpdateTransaction(int id, DateTime date, decimal amount, TransactionType type, string category, string description)
    {
        var t = transactions.FirstOrDefault(t => t.Id == id);
        if (t == null)
        {
            Console.WriteLine($"Transaction with ID {id} not found.");
            return;
        }

        t.Date = date;
        t.Amount = amount;
        t.Type = type;
        t.Category = category;
        t.Description = description;
        Console.WriteLine("Transaction updated successfully.");
    }

    // Displays all stored transactions
    public void ListTransactions()
    {
        if (!transactions.Any())
        {
            Console.WriteLine("No transactions found.");
            return;
        }

        Console.WriteLine("  ID | Date       | Type    | Category      |     Amount | Description");
        Console.WriteLine("--------------------------------------------------------------------------");

        foreach (var t in transactions)
        {
            t.Display();
        }
    }

    // Displays income, expense, balance, and totals per category
    public void ShowSummary()
    {
        decimal totalIncome = transactions
            .Where(t => t.Type == TransactionType.Income)
            .Sum(t => t.Amount);

        decimal totalExpense = transactions
            .Where(t => t.Type == TransactionType.Expense)
            .Sum(t => t.Amount);

        decimal balance = totalIncome - totalExpense;

        Console.WriteLine("\n----- Summary -----");
        Console.WriteLine($"Total Income: {totalIncome:C}");
        Console.WriteLine($"Total Expense: {totalExpense:C}");
        Console.WriteLine($"Balance: {balance:C}");

        Console.WriteLine("\nTotals by Category:");

        var categoryTotals = transactions
            .GroupBy(t => t.Category)
            .Select(g => new { Category = g.Key, Total = g.Sum(t => t.Amount) });

        foreach (var c in categoryTotals)
        {
            Console.WriteLine($"{c.Category}: {c.Total:C}");
        }
    }

    // Saves all transactions to a CSV file
    public void SaveToCsv(string filePath)
    {
        try
        {
            using (var writer = new StreamWriter(filePath))
            {
                // CSV header
                writer.WriteLine("Date,Type,Category,Amount,Description");

                foreach (var t in transactions)
                {
                    writer.WriteLine($"{t.Date:yyyy-MM-dd},{t.Type},{t.Category},{t.Amount},{t.Description}");
                }
            }

            Console.WriteLine($"Transactions saved to {filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving to CSV: {ex.Message}");
        }
    }

    // Loads transactions from a CSV file
    public void LoadFromCsv(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"File {filePath} does not exist.");
                return;
            }

            transactions.Clear();
            var lines = File.ReadAllLines(filePath);

            // CSV must contain at least header + one data line
            if (lines.Length <= 1)
            {
                Console.WriteLine("CSV file is empty or only contains header.");
                return;
            }

            foreach (var line in lines.Skip(1)) // Skip header
            {
                var parts = line.Split(',');

                // Guard against malformed rows
                if (parts.Length != 5)
                    continue;

                if (DateTime.TryParse(parts[0], out DateTime date) &&
                    Enum.TryParse(parts[1], out TransactionType type) &&
                    decimal.TryParse(parts[3], out decimal amount))
                {
                    string category = parts[2];
                    string description = parts[4];

                    transactions.Add(
                        new Transaction(nextId++, date, amount, type, category, description)
                    );
                }
                else
                {
                    Console.WriteLine($"Skipped invalid line: {line}");
                }
            }

            Console.WriteLine($"Transactions loaded from {filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading CSV: {ex.Message}");
        }
    }
}

//
// APPLICATION ENTRY POINT
// --------------------------------------------------

class Program
{
    static void Main()
    {
        FinanceManager manager = new FinanceManager();

        // Attempt to load default data on startup
        string defaultFile = "transactions.csv";
        manager.LoadFromCsv(defaultFile);

        bool running = true;

        while (running)
        {
            Console.WriteLine("\n1. Add Transaction");
            Console.WriteLine("2. View All Transactions");
            Console.WriteLine("3. View Summary");
            Console.WriteLine("4. Edit Transaction");
            Console.WriteLine("5. Delete Transaction");
            Console.WriteLine("6. Save Transactions to CSV");
            Console.WriteLine("7. Load Transactions from CSV");
            Console.WriteLine("8. Exit");

            string input = Console.ReadLine()!;

            switch (input)
            {
                case "1":
                    AddTransactionFlow(manager);
                    break;

                case "2":
                    manager.ListTransactions();
                    break;

                case "3":
                    manager.ShowSummary();
                    break;

                case "4":
                    EditTransactionFlow(manager);
                    break;

                case "5":
                    DeleteTransactionFlow(manager);
                    break;

                case "6":
                    manager.SaveToCsv(
                        GetValidFileName("Enter file name to save (e.g., transactions.csv): ")
                    );
                    break;

                case "7":
                    manager.LoadFromCsv(
                        GetValidFileName("Enter file name to load (e.g., transactions.csv): ")
                    );
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

    // Ensures a valid file name is provided before save/load
    static string GetValidFileName(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string input = Console.ReadLine()!.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("File name cannot be empty. Using default: transactions.csv");
                return "transactions.csv";
            }

            if (input.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                Console.WriteLine("File name contains invalid characters.");
                continue;
            }

            return input;
        }
    }

    // Handles the full transaction input flow with validation
    static void AddTransactionFlow(FinanceManager manager)
    {
        bool adding = true;

        while (adding)
        {
            DateTime date;
            decimal amount;
            TransactionType type;
            string category = "";
            string description;

            // Date input
            while (true)
            {
                Console.Write("Enter date (yyyy-MM-dd): ");
                if (DateTime.TryParse(Console.ReadLine(), out date))
                    break;

                Console.WriteLine("Invalid date format.");
            }

            // Amount input
            while (true)
            {
                Console.Write("Enter amount: ");
                if (decimal.TryParse(Console.ReadLine(), out amount) && amount >= 0)
                    break;

                Console.WriteLine("Invalid amount.");
            }

            // Transaction type
            while (true)
            {
                Console.WriteLine("Select type: 1. Income  2. Expense");
                string input = Console.ReadLine()!;

                if (input == "1") { type = TransactionType.Income; break; }
                if (input == "2") { type = TransactionType.Expense; break; }

                Console.WriteLine("Invalid selection.");
            }

            // Category selection
            while (true)
            {
                if (type == TransactionType.Expense)
                {
                    Console.WriteLine("Select Expense category:");
                    foreach (var cat in Enum.GetValues(typeof(ExpenseCategory)))
                        Console.WriteLine($"{(int)cat + 1}. {cat}");

                    if (int.TryParse(Console.ReadLine(), out int idx) &&
                        idx >= 1 && idx <= Enum.GetValues(typeof(ExpenseCategory)).Length)
                    {
                        category = ((ExpenseCategory)(idx - 1)).ToString();
                        break;
                    }
                }
                else
                {
                    Console.WriteLine("Select Income category:");
                    foreach (var cat in Enum.GetValues(typeof(IncomeCategory)))
                        Console.WriteLine($"{(int)cat + 1}. {cat}");

                    if (int.TryParse(Console.ReadLine(), out int idx) &&
                        idx >= 1 && idx <= Enum.GetValues(typeof(IncomeCategory)).Length)
                    {
                        category = ((IncomeCategory)(idx - 1)).ToString();
                        break;
                    }
                }

                Console.WriteLine("Invalid category.");
            }

            // Description
            while (true)
            {
                Console.Write("Enter description: ");
                description = Console.ReadLine()!;

                if (!string.IsNullOrWhiteSpace(description))
                    break;

                Console.WriteLine("Description cannot be empty.");
            }

        manager.AddTransaction(date, amount, type, category, description);

            // Continue?
            while (true)
            {
                Console.Write("Add another transaction? (y/n): ");
                string cont = Console.ReadLine()!.Trim().ToLower();

                if (cont == "y") break;
                if (cont == "n") { adding = false; break; }

                Console.WriteLine("Invalid input.");
            }
        }
    }

    // Handles deletion of a transaction by ID
    static void DeleteTransactionFlow(FinanceManager manager)
    {
        manager.ListTransactions();

        while (true)
        {
            Console.Write("\nEnter transaction ID to delete (or 0 to cancel): ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                if (id == 0)
                {
                    Console.WriteLine("Cancelled.");
                    return;
                }

                if (manager.DeleteTransaction(id))
                    return;
            }
            else
            {
                Console.WriteLine("Invalid ID.");
            }
        }
    }

    // Handles editing of a transaction by ID
    static void EditTransactionFlow(FinanceManager manager)
    {
        manager.ListTransactions();

        Console.Write("\nEnter transaction ID to edit (or 0 to cancel): ");
        if (!int.TryParse(Console.ReadLine(), out int id) || id == 0)
        {
            Console.WriteLine("Cancelled.");
            return;
        }

        var existing = manager.GetTransactionById(id);
        if (existing == null)
        {
            Console.WriteLine($"Transaction with ID {id} not found.");
            return;
        }

        Console.WriteLine($"\nEditing transaction {id}. Press Enter to keep current value.");

        // Date
        Console.Write($"Date ({existing.Date:yyyy-MM-dd}): ");
        string dateInput = Console.ReadLine()!;
        DateTime date = string.IsNullOrWhiteSpace(dateInput) ? existing.Date : DateTime.Parse(dateInput);

        // Amount
        Console.Write($"Amount ({existing.Amount}): ");
        string amountInput = Console.ReadLine()!;
        decimal amount = string.IsNullOrWhiteSpace(amountInput) ? existing.Amount : decimal.Parse(amountInput);

        // Type
        Console.Write($"Type ({existing.Type}) - 1. Income  2. Expense: ");
        string typeInput = Console.ReadLine()!;
        TransactionType type = existing.Type;
        if (typeInput == "1") type = TransactionType.Income;
        else if (typeInput == "2") type = TransactionType.Expense;

        // Category
        string category = existing.Category;
        Console.Write($"Change category? Current: {existing.Category} (y/n): ");
        if (Console.ReadLine()!.Trim().ToLower() == "y")
        {
            if (type == TransactionType.Expense)
            {
                Console.WriteLine("Select Expense category:");
                foreach (var cat in Enum.GetValues(typeof(ExpenseCategory)))
                    Console.WriteLine($"{(int)cat + 1}. {cat}");

                if (int.TryParse(Console.ReadLine(), out int idx) &&
                    idx >= 1 && idx <= Enum.GetValues(typeof(ExpenseCategory)).Length)
                {
                    category = ((ExpenseCategory)(idx - 1)).ToString();
                }
            }
            else
            {
                Console.WriteLine("Select Income category:");
                foreach (var cat in Enum.GetValues(typeof(IncomeCategory)))
                    Console.WriteLine($"{(int)cat + 1}. {cat}");

                if (int.TryParse(Console.ReadLine(), out int idx) &&
                    idx >= 1 && idx <= Enum.GetValues(typeof(IncomeCategory)).Length)
                {
                    category = ((IncomeCategory)(idx - 1)).ToString();
                }
            }
        }

        // Description
        Console.Write($"Description ({existing.Description}): ");
        string descInput = Console.ReadLine()!;
        string description = string.IsNullOrWhiteSpace(descInput) ? existing.Description : descInput;

        manager.UpdateTransaction(id, date, amount, type, category, description);
    }
}
