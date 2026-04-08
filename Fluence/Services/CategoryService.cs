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
            var db = await GetDbAsync();
            return await db.Table<Category>().OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<Category> GetCategoryByIdAsync(int id)
        {
            var db = await GetDbAsync();
            return await db.Table<Category>().Where(c => c.Id == id).FirstOrDefaultAsync();
        }

        public async Task AddCategoryAsync(Category category)
        {
            var db = await GetDbAsync();
            await db.InsertAsync(category);    
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            var db = await GetDbAsync();
            await db.UpdateAsync(category);
        }

        public async Task DeleteCategoryAsync(Category category)
        {
            var db = await GetDbAsync();
            await db.DeleteAsync(category);
        }

        public async Task InitializeDatabaseSync()
        {
            var db = await GetDbAsync();
            if (await db.Table<Category>().CountAsync() == 0)
            {
                List<Category> systemCategories = new List<Category>
                {
                    new Category { Name = "Food", IsSystem = true },
                    new Category { Name = "Transportation", IsSystem = true },
                    new Category { Name = "Bills", IsSystem = true },
                    new Category { Name = "Income", IsSystem = true },
                    new Category { Name = "Expense", IsSystem = true },
                };

                await db.InsertAllAsync(systemCategories);
            }
        }
    }
}
