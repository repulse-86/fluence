using Fluence.Services;
using Fluence.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;

namespace Fluence.ViewModels
{
    public class CategoryViewModel : INotifyPropertyChanged
    {
        private readonly CategoryService _categoryService = new CategoryService();
        private bool _isBusy;
        private Category _selectedCategory;
        private string _categoryName;
        private string _errorMessage;
        private bool _isEditing;

        public bool IsEditing
        {
            get { return _isEditing; }
            set
            {
                if (_isEditing != value)
                {
                    _isEditing = value;
                    OnPropertyChanged();
                    OnPropertyChanged("ViewVisibility");
                    OnPropertyChanged("EditVisibility");
                    OnPropertyChanged("SaveButtonLabel");
                    OnPropertyChanged("SaveButtonSymbol");
                }
            }
        }

        public Windows.UI.Xaml.Visibility ViewVisibility => IsEditing ? Windows.UI.Xaml.Visibility.Collapsed : Windows.UI.Xaml.Visibility.Visible;
        public Windows.UI.Xaml.Visibility EditVisibility => IsEditing ? Windows.UI.Xaml.Visibility.Visible : Windows.UI.Xaml.Visibility.Collapsed;

        public string SaveButtonLabel => IsEditing ? "save" : "add";
        public Symbol SaveButtonSymbol => IsEditing ? Symbol.Save : Symbol.Add;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Category> Categories { get; set; } = new ObservableCollection<Category>();

        public Category SelectedCategory
        {
            get { return _selectedCategory; }
            set 
            { 
                if (_selectedCategory != value)
                {
                    _selectedCategory = value; 
                    OnPropertyChanged(); 
                }
            }
        }

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
                ErrorMessage = "failed to load categories: " + ex.Message;
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
                ErrorMessage = "category name is required.";
                return;
            }

            if (IsBusy) return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                if (SelectedCategory == null)
                {
                    Category newCategory = new Category { Name = CategoryName.Trim().ToLower() };
                    await _categoryService.AddCategoryAsync(newCategory);
                    Categories.Add(newCategory);
                }
                else
                {
                    SelectedCategory.Name = CategoryName.Trim().ToLower();
                    await _categoryService.UpdateCategoryAsync(SelectedCategory);
                    SelectedCategory = null;
                }
                CategoryName = string.Empty;
                IsEditing = false;
            }
            catch (Exception ex)
            {
                ErrorMessage = "failed to save: " + ex.Message;
                throw;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task DeleteCategoryAsync(Category category)
        {
            if (category == null) return;
            if (IsBusy) return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                await _categoryService.DeleteCategoryAsync(category);
                Categories.Remove(category);

                if (SelectedCategory == category)
                {
                    SelectedCategory = null;
                    CategoryName = string.Empty;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "failed to delete: " + ex.Message;
                throw;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
