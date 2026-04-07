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
            await InitializeDatabaseAsync();
            return await _db.Table<Transaction>().OrderByDescending(t => t.Date).ToListAsync();
        }

        public async Task AddTransactionAsync(Transaction transaction)
        {
            await InitializeDatabaseAsync();
            await _db.InsertAsync(transaction);
        }

        public async Task UpdateTransactionAsync(Transaction transaction)
        {
            await InitializeDatabaseAsync();
            await _db.UpdateAsync(transaction);
        }

        public async Task<Transaction> GetTransactionByIdAsync(int id)
        {
            await InitializeDatabaseAsync();
            return await _db.Table<Transaction>().Where(t => t.Id == id).FirstOrDefaultAsync();
        }

        public async Task DeleteTransactionAsync(int id)
        {
            await InitializeDatabaseAsync();
            var transaction = await GetTransactionByIdAsync(id);
            if (transaction != null)
            {
                await _db.DeleteAsync(transaction);
            }
        }

        public async Task DeleteTransactionAsync(Transaction transaction)
        {
            await InitializeDatabaseAsync();
            await _db.DeleteAsync(transaction);
        }

        public async Task SeedTransactionsAsync()
        {
            await InitializeDatabaseAsync();
            
            await _db.ExecuteAsync("DELETE FROM [Transaction]");

            var categories = await _db.Table<Category>().ToListAsync();
            if (categories.Count == 0) return;

            var random = new Random();
            var transactions = new List<Transaction>();

            for (int d = 1; d <= 7; d++)
            {
                var baseDate = new DateTime(2026, 4, d);

                for (int i = 0; i < 3; i++)
                {
                    var category = categories[random.Next(categories.Count)];
                    var type = random.Next(2) == 0 ? "Income" : "Expense";
                    var amount = Math.Round(random.NextDouble() * 500 + 10, 2);
                    
                    var transactionDate = baseDate.AddHours(10 + i).AddMinutes(random.Next(0, 60));

                    transactions.Add(new Transaction
                    {
                        CategoryId = category.Id,
                        Amount = amount,
                        Type = type,
                        Note = "grouped transaction " + baseDate.ToString("mmm dd") + " #" + (i + 1),
                        Date = transactionDate
                    });
                }
            }

            await _db.InsertAllAsync(transactions);
        }
    }
}
