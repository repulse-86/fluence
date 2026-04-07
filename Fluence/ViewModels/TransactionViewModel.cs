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
    class TransactionViewModel : INotifyPropertyChanged
    {
        private readonly TransactionService _transactionService = new TransactionService();
        private readonly CategoryService _categoryService = new CategoryService();

        private bool _isBusy;
        private string _amountText;
        private string _note;
        private int _categoryId;
        private string _type;
        private string _errorMessage;
        private string _debugData;
        private Windows.UI.Xaml.Media.Brush _debugColor = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Gray);

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Transaction> Transactions { get; set; } = new ObservableCollection<Transaction>();
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

        public string AmountText
        {
            get { return _amountText; }
            set
            {
                if (_amountText != value)
                {
                    _amountText = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Note
        {
            get { return _note; }
            set
            {
                if (_note != value)
                {
                    _note = value;
                    OnPropertyChanged();
                }
            }
        }

        public int CategoryId
        {
            get { return _categoryId; }
            set
            {
                if (_categoryId != value)
                {
                    _categoryId = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Type
        {
            get { return _type; }
            set
            {
                if (_type != value)
                {
                    _type = value;
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

        public string DebugData
        {
            get { return _debugData; }
            set
            {
                if (_debugData != value)
                {
                    _debugData = value;
                    OnPropertyChanged();
                }
            }
        }

        public Windows.UI.Xaml.Media.Brush DebugColor
        {
            get { return _debugColor; }
            set
            {
                if (_debugColor != value)
                {
                    _debugColor = value;
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
            }
        }

        public async Task LoadTransactionsAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var transactions = await _transactionService.GetTransactionsAsync();
                Transactions.Clear();
                foreach (var transaction in transactions)
                {
                    Transactions.Add(transaction);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Failed to load transactions: " + ex.Message;
            }
            finally { IsBusy = false; }
        }

        public async Task<bool> AddTransactionAsync()
        {
            ErrorMessage = string.Empty;

            double amount;
            if (!double.TryParse(AmountText, out amount) || amount <= 0)
            {
                ErrorMessage = "Amount must be a positive number.";
                return false;
            }

            if (CategoryId == 0)
            {
                ErrorMessage = "Please select a category.";
                return false;
            }

            if (string.IsNullOrEmpty(Type))
            {
                ErrorMessage = "Please select a transaction type.";
                return false;
            }

            if (IsBusy) return false;
            IsBusy = true;

            try
            {
                var transaction = new Transaction
                {
                    Amount = amount,
                    Note = Note,
                    CategoryId = CategoryId,
                    Type = Type,
                    Date = DateTime.Now
                };

                await _transactionService.AddTransactionAsync(transaction);
                Transactions.Insert(0, transaction);
                
                DebugData = "saved successfully";
                DebugColor = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Green);

                // Clear inputs
                AmountText = string.Empty;
                Note = string.Empty;
                
                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = "Failed to save transaction: " + ex.Message;
                return false;
            }
            finally { IsBusy = false; }
        }
    }
}
