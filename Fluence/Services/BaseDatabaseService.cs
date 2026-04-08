using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using Windows.Storage;
using System.IO;
using Fluence.Models;

namespace Fluence.Services
{
    class BaseDatabaseService
    {
        private static SQLiteAsyncConnection _db;

        protected async Task<SQLiteAsyncConnection> GetDbAsync()
        {
            if (_db == null)
            {
                string folderPath = ApplicationData.Current.LocalFolder.Path;
                string path = Path.Combine(folderPath, "Fluence.db");
                _db = new SQLiteAsyncConnection(path);

                await _db.CreateTableAsync<Category>();
                await _db.CreateTableAsync<Transaction>();
            }
            return _db;
        }

        public async Task InitializeDatabaseAsync()
        {
            await GetDbAsync();
        }
    }
}
