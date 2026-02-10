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
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public string Category { get; set; }
    public string Description { get; set; }

    public Transaction(DateTime date, decimal amount, TransactionType type, string category, string description)
    {
        Date = date;
        Amount = amount;
        Type = type;
        Category = category;
        Description = description;
    }

    // Displays the transaction in a formatted console-friendly line
    public void Display()
    {
        Console.WriteLine($"{Date:yyyy-MM-dd} | {Type} | {Category} | {Amount:C} | {Description}");
    }
}

//
// BUSINESS LOGIC
// --------------------------------------------------

// Manages transactions and persistence logic
class FinanceManager
{
    private List<Transaction> transactions;

    public FinanceManager()
    {
        transactions = new List<Transaction>();
    }

    // Adds a new transaction to memory
    public void AddTransaction(Transaction t)
    {
        transactions.Add(t);
        Console.WriteLine("Transaction added successfully.");
    }

    // Displays all stored transactions
    public void ListTransactions()
    {
        if (!transactions.Any())
        {
            Console.WriteLine("No transactions found.");
            return;
        }

        Console.WriteLine("Date       | Type    | Category      | Amount   | Description");
        Console.WriteLine("--------------------------------------------------------------");

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
                        new Transaction(date, amount, type, category, description)
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
            Console.WriteLine("4. Save Transactions to CSV");
            Console.WriteLine("5. Load Transactions from CSV");
            Console.WriteLine("6. Exit");

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
                    manager.SaveToCsv(
                        GetValidFileName("Enter file name to save (e.g., transactions.csv): ")
                    );
                    break;

                case "5":
                    manager.LoadFromCsv(
                        GetValidFileName("Enter file name to load (e.g., transactions.csv): ")
                    );
                    break;

                case "6":
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

            manager.AddTransaction(
                new Transaction(date, amount, type, category, description)
            );

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
}
