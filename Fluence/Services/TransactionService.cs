using Fluence.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fluence.Services
{
    class TransactionService : BaseDatabaseService
    {
        public async Task<List<Transaction>> GetTransactionsAsync()
        {
            var db = await GetDbAsync();
            return await db.Table<Transaction>().OrderByDescending(t => t.Date).ToListAsync();
        }

        public async Task<double> GetTotalIncomeAsync()
        {
            var db = await GetDbAsync();
            var result = await db.ExecuteScalarAsync<double>("SELECT COALESCE(SUM(Amount), 0) FROM [Transaction] WHERE Type = 'Income'");
            return result;
        }

        public async Task<double> GetTotalExpenseAsync()
        {
            var db = await GetDbAsync();
            var result = await db.ExecuteScalarAsync<double>("SELECT COALESCE(SUM(Amount), 0) FROM [Transaction] WHERE Type = 'Expense'");
            return result;
        }

        public async Task<double> GetMonthlyExpenseAsync(int year, int month)
        {
            var db = await GetDbAsync();
            DateTime start = new DateTime(year, month, 1);
            DateTime end = start.AddMonths(1);
            var result = await db.ExecuteScalarAsync<double>("SELECT COALESCE(SUM(Amount), 0) FROM [Transaction] WHERE Type = 'Expense' AND Date >= ? AND Date < ?", start.Ticks, end.Ticks);
            return result;
        }

        public async Task<double> GetExpenseSumAsync(DateTime start, DateTime end)
        {
            var db = await GetDbAsync();
            return await db.ExecuteScalarAsync<double>("SELECT COALESCE(SUM(Amount), 0) FROM [Transaction] WHERE Type = 'Expense' AND Date >= ? AND Date < ?", start.Ticks, end.Ticks);
        }

        public async Task<double> GetIncomeSumAsync(DateTime start, DateTime end)
        {
            var db = await GetDbAsync();
            return await db.ExecuteScalarAsync<double>("SELECT COALESCE(SUM(Amount), 0) FROM [Transaction] WHERE Type = 'Income' AND Date >= ? AND Date < ?", start.Ticks, end.Ticks);
        }

        public async Task<List<Transaction>> GetTransactionsAsync(DateTime start, DateTime end)
        {
            var db = await GetDbAsync();
            return await db.Table<Transaction>()
                .Where(t => t.Date >= start && t.Date < end)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
        }

        public class CategorySummary
        {
            public int CategoryId { get; set; }
            public double TotalAmount { get; set; }
            public int Count { get; set; }
        }

        public async Task<List<CategorySummary>> GetCategorySummariesAsync(DateTime start, DateTime end, string type = "Expense")
        {
            var db = await GetDbAsync();
            // SQLite-net-pcl doesn't support complex group by with projections well in LINQ, use raw SQL
            string sql = "SELECT CategoryId, SUM(Amount) as TotalAmount, COUNT(Id) as Count FROM [Transaction] WHERE Type = ? AND Date >= ? AND Date < ? GROUP BY CategoryId";
            return await db.QueryAsync<CategorySummary>(sql, type, start.Ticks, end.Ticks);
        }

        public async Task RunMigrationsAsync()
        {
            var db = await GetDbAsync();
            var existingTransactions = await db.Table<Transaction>().ToListAsync();
            foreach (var t in existingTransactions)
            {
                if (t.Note != null && t.Note != t.Note.ToLower())
                {
                    t.Note = t.Note.ToLower();
                    await db.UpdateAsync(t);
                }
            }
        }

        public async Task<List<Transaction>> GetTransactionsAsync(int skip, int take)
        {
            var db = await GetDbAsync();
            return await db.Table<Transaction>()
                .OrderByDescending(t => t.Date)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task AddTransactionAsync(Transaction transaction)
        {
            if (transaction != null && transaction.Note != null)
                transaction.Note = transaction.Note.ToLower();

            var db = await GetDbAsync();
            await db.InsertAsync(transaction);
        }

        public async Task UpdateTransactionAsync(Transaction transaction)
        {
            if (transaction != null && transaction.Note != null)
                transaction.Note = transaction.Note.ToLower();

            var db = await GetDbAsync();
            await db.UpdateAsync(transaction);
        }

        public async Task<Transaction> GetTransactionByIdAsync(int id)
        {
            var db = await GetDbAsync();
            return await db.Table<Transaction>().Where(t => t.Id == id).FirstOrDefaultAsync();
        }

        public async Task DeleteTransactionAsync(int id)
        {
            var transaction = await GetTransactionByIdAsync(id);
            if (transaction != null)
            {
                var db = await GetDbAsync();
                await db.DeleteAsync(transaction);
            }
        }

        public async Task DeleteTransactionAsync(Transaction transaction)
        {
            var db = await GetDbAsync();
            await db.DeleteAsync(transaction);
        }

        public async Task ClearTransactionsAsync()
        {
            var db = await GetDbAsync();
            await db.ExecuteAsync("DELETE FROM [Transaction]");
        }

        public async Task SeedTransactionsAsync()
        {
            var db = await GetDbAsync();
            await ClearTransactionsAsync();
            
            var categories = await db.Table<Category>().ToListAsync();
            if (categories.Count == 0) return;

            var random = new Random();
            var transactions = new List<Transaction>();
            var today = DateTime.Now.Date;
            
            for (int d = 0; d < 30; d++)
            {
                var baseDate = today.AddDays(-d);

                for (int i = 0; i < 5; i++)
                {
                    var category = categories[random.Next(categories.Count)];
                    
                    string type = (i == 0) ? "Income" : "Expense";
                    double amount = (i == 0) 
                        ? Math.Round(random.NextDouble() * 400 + 600, 2)
                        : Math.Round(random.NextDouble() * 80 + 10, 2);
                    
                    var transactionDate = baseDate.AddHours(8 + (i * 2)).AddMinutes(random.Next(0, 60));

                    transactions.Add(new Transaction
                    {
                        CategoryId = category.Id,
                        Amount = amount,
                        Type = type,
                        Note = ("mock " + type.ToLower() + " " + baseDate.ToString("MMM dd") + " #" + (i + 1)).ToLower(),
                        Date = transactionDate
                    });
                }
            }

            await db.InsertAllAsync(transactions);
        }
    }
}
