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
            var db = await GetDbAsync();
            await db.InsertAsync(transaction);
        }

        public async Task UpdateTransactionAsync(Transaction transaction)
        {
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
                        Note = "mock " + type.ToLower() + " " + baseDate.ToString("MMM dd") + " #" + (i + 1),
                        Date = transactionDate
                    });
                }
            }

            await db.InsertAllAsync(transactions);
        }
    }
}
