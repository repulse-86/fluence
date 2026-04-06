using Fluence.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fluence.Services
{
    class CategoryService : BaseDatabaseService
    {
        public async Task<List<Category>> GetCategoriesAsync()
        {
            await InitializeDatabaseAsync();
            return await _db.Table<Category>().OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<Category> GetCategoryByIdAsync(int id)
        {
            await InitializeDatabaseAsync();
            return await _db.Table<Category>().Where(c => c.Id == id).FirstOrDefaultAsync();
        }

        public async Task AddCategoryAsync(Category category)
        {
            await InitializeDatabaseAsync();
            await _db.InsertAsync(category);    
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            await InitializeDatabaseAsync();
            await _db.UpdateAsync(category);
        }

        public async Task DeleteCategoryAsync(Category category)
        {
            await InitializeDatabaseAsync();
            await _db.DeleteAsync(category);
        }

        public async Task InitializeDatabaseSync()
        {
            await InitializeDatabaseAsync();
            if (await _db.Table<Category>().CountAsync() == 0)
            {
                List<Category> systemCategories = new List<Category>
                {
                    new Category { Name = "Food", IsSystem = true },
                    new Category { Name = "Transportation", IsSystem = true },
                    new Category { Name = "Bills", IsSystem = true },
                    new Category { Name = "Income", IsSystem = true },
                    new Category { Name = "Expense", IsSystem = true },
                };

                await _db.InsertAllAsync(systemCategories);
            }
        }
    }
}
