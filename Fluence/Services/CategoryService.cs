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
            if (category == null) return;
            if (category.Name != null) category.Name = category.Name.Trim().ToLower();

            var db = await GetDbAsync();
            await db.InsertAsync(category);    
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            if (category == null) return;
            if (category.Name != null) category.Name = category.Name.Trim().ToLower();

            var db = await GetDbAsync();
            await db.UpdateAsync(category);
        }

        public async Task DeleteCategoryAsync(Category category)
        {
            if (category == null) return;
            var db = await GetDbAsync();
            await db.DeleteAsync(category);
        }

        public async Task InitializeDatabaseSync()
        {
            var db = await GetDbAsync();

            var existingCategories = await db.Table<Category>().ToListAsync();
            foreach (var cat in existingCategories)
            {
                if (cat.Name != cat.Name.ToLower())
                {
                    cat.Name = cat.Name.ToLower();
                    await db.UpdateAsync(cat);
                }
            }

            if (await db.Table<Category>().CountAsync() == 0)
            {
                List<Category> systemCategories = new List<Category>
                {
                    new Category { Name = "salary", IsSystem = true },
                    new Category { Name = "food & dining", IsSystem = true },
                    new Category { Name = "transportation", IsSystem = true },
                    new Category { Name = "housing & rent", IsSystem = true },
                    
                    new Category { Name = "utilities", IsSystem = true },
                    new Category { Name = "subscriptions", IsSystem = true },

                    new Category { Name = "shopping", IsSystem = true },
                    new Category { Name = "entertainment", IsSystem = true },
                    new Category { Name = "health & fitness", IsSystem = true },

                    new Category { Name = "savings & investment", IsSystem = true },
                    new Category { Name = "debt repayment", IsSystem = true },
                    new Category { Name = "others", IsSystem = true },
                };

                await db.InsertAllAsync(systemCategories);
            }
        }
    }
}
