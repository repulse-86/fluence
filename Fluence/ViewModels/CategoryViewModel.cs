using Fluence.Services;
using Fluence.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Fluence.ViewModels
{
    class CategoryViewModel : INotifyPropertyChanged
    {
        private readonly CategoryService _categoryService = new CategoryService();
        private bool _isBusy;
        private string _categoryName;
        private string _errorMessage;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Category> Categories { get; set; } = new ObservableCollection<Category>();

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    OnPropertyChanged();
                }
            }
        }

        public string CategoryName
        {
            get { return _categoryName; }
            set
            {
                if (_categoryName != value)
                {
                    _categoryName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                if (_errorMessage != value)
                {
                    _errorMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task LoadCategoriesAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                var categories = await _categoryService.GetCategoriesAsync();
                Categories.Clear();
                foreach (var category in categories)
                {
                    Categories.Add(category);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Failed to load categories: " + ex.Message;
                throw;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task SaveCategoryAsync()
        {
            if (string.IsNullOrWhiteSpace(CategoryName))
            {
                ErrorMessage = "Category name is required.";
                throw new ArgumentException("Category name is required.");
            }

            if (IsBusy) return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                Category newCategory = new Category { Name = CategoryName };
                await _categoryService.AddCategoryAsync(newCategory);
                Categories.Add(newCategory);
                CategoryName = string.Empty;
            }
            catch (Exception ex)
            {
                ErrorMessage = "Failed to save: " + ex.Message;
                throw;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
