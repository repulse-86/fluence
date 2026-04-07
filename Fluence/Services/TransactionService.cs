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

        public async Task DeleteTransactionAsync(Transaction transaction)
        {
            await InitializeDatabaseAsync();
            await _db.DeleteAsync(transaction);
        }
    }
}
