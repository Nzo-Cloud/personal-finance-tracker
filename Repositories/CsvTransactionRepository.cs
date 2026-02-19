using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Repositories;

/// <summary>
/// CSV file-based repository for transaction persistence.
/// Wraps an in-memory repository and syncs to/from CSV files.
/// </summary>
public class CsvTransactionRepository : ITransactionRepository
{
    private readonly InMemoryTransactionRepository _memoryRepo = new();

    public Transaction Add(Transaction transaction) => _memoryRepo.Add(transaction);
    public bool Update(Transaction transaction) => _memoryRepo.Update(transaction);
    public bool Delete(int id) => _memoryRepo.Delete(id);
    public Transaction? GetById(int id) => _memoryRepo.GetById(id);
    public IReadOnlyList<Transaction> GetAll() => _memoryRepo.GetAll();
    public void Clear() => _memoryRepo.Clear();

    /// <summary>
    /// Saves all transactions to a CSV file.
    /// </summary>
    /// <returns>Error message if failed, null if successful.</returns>
    public string? SaveToFile(string filePath)
    {
        try
        {
            using var writer = new StreamWriter(filePath);
            writer.WriteLine("Id,Date,Type,ExpenseCategory,IncomeCategory,Amount,Description");

            foreach (var t in _memoryRepo.GetAll())
            {
                var expenseCat = t.ExpenseCategory?.ToString() ?? "";
                var incomeCat = t.IncomeCategory?.ToString() ?? "";
                var description = EscapeCsvField(t.Description);
                writer.WriteLine($"{t.Id},{t.Date:yyyy-MM-dd},{t.Type},{expenseCat},{incomeCat},{t.Amount},{description}");
            }

            return null;
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    /// <summary>
    /// Loads transactions from a CSV file, replacing current data.
    /// </summary>
    /// <returns>Result with success status and message.</returns>
    public (bool Success, string Message, int Count) LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            return (false, $"File '{filePath}' does not exist.", 0);

        try
        {
            var lines = File.ReadAllLines(filePath);
            if (lines.Length <= 1)
                return (false, "CSV file is empty or only contains header.", 0);

            _memoryRepo.Clear();
            int maxId = 0;
            int loadedCount = 0;
            var skippedLines = new List<string>();

            foreach (var line in lines.Skip(1))
            {
                var parts = ParseCsvLine(line);
                if (parts.Length < 7)
                {
                    skippedLines.Add(line);
                    continue;
                }

                if (!int.TryParse(parts[0], out int id) ||
                    !DateTime.TryParse(parts[1], out DateTime date) ||
                    !Enum.TryParse<TransactionType>(parts[2], out var type) ||
                    !decimal.TryParse(parts[5], out decimal amount))
                {
                    skippedLines.Add(line);
                    continue;
                }

                var transaction = new Transaction(id, date, amount, type, parts[6]);

                if (type == TransactionType.Expense && Enum.TryParse<ExpenseCategory>(parts[3], out var expCat))
                    transaction.ExpenseCategory = expCat;
                else if (type == TransactionType.Income && Enum.TryParse<IncomeCategory>(parts[4], out var incCat))
                    transaction.IncomeCategory = incCat;

                // Directly add to internal list to preserve ID
                _memoryRepo.Add(transaction);
                transaction.Id = id; // Override the auto-generated ID
                maxId = Math.Max(maxId, id);
                loadedCount++;
            }

            _memoryRepo.SetNextId(maxId + 1);

            var message = $"Loaded {loadedCount} transactions.";
            if (skippedLines.Count > 0)
                message += $" Skipped {skippedLines.Count} invalid lines.";

            return (true, message, loadedCount);
        }
        catch (Exception ex)
        {
            return (false, $"Error loading CSV: {ex.Message}", 0);
        }
    }

    private static string EscapeCsvField(string field)
    {
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }
        return field;
    }

    private static string[] ParseCsvLine(string line)
    {
        var result = new List<string>();
        var current = "";
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current += '"';
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    current += c;
                }
            }
            else
            {
                if (c == '"')
                {
                    inQuotes = true;
                }
                else if (c == ',')
                {
                    result.Add(current);
                    current = "";
                }
                else
                {
                    current += c;
                }
            }
        }

        result.Add(current);
        return result.ToArray();
    }
}
