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
        protected static SQLiteAsyncConnection _db;

        public async Task InitializeDatabaseAsync()
        {
            if (_db != null) return;

            string folderPath = ApplicationData.Current.LocalFolder.Path;
            string path = Path.Combine(folderPath, "Fluence.db");
            _db = new SQLiteAsyncConnection(path);

            await _db.CreateTableAsync<Category>();
        }
    }
}
