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
    public class TransactionViewModel : INotifyPropertyChanged
    {
        private readonly TransactionService _transactionService = new TransactionService();
        private readonly CategoryService _categoryService = new CategoryService();
        private readonly WalletService _walletService = new WalletService();

        private bool _isBusy;
        private string _amountText;
        private string _note;
        private int _categoryId;
        private int _walletId;
        private string _type;
        private string _errorMessage;
        private string _debugData;
        private string _headerTitle = "quick add";
        private Transaction _selectedTransaction;
        private Windows.UI.Xaml.Media.Brush _debugColor = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Gray);

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Transaction> Transactions { get; set; } = new ObservableCollection<Transaction>();
        public ObservableCollection<Category> Categories { get; set; } = new ObservableCollection<Category>();
        public ObservableCollection<Wallet> Wallets { get; set; } = new ObservableCollection<Wallet>();

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

        public int WalletId
        {
            get { return _walletId; }
            set
            {
                if (_walletId != value)
                {
                    _walletId = value;
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

        public string HeaderTitle
        {
            get { return _headerTitle; }
            set
            {
                if (_headerTitle != value)
                {
                    _headerTitle = value;
                    OnPropertyChanged();
                }
            }
        }

        public Transaction SelectedTransaction
        {
            get { return _selectedTransaction; }
            set
            {
                _selectedTransaction = value;
                if (_selectedTransaction != null)
                {
                    HeaderTitle = "edit transaction";
                    AmountText = _selectedTransaction.Amount.ToString(AppConstants.CurrencyFormat);
                    Note = _selectedTransaction.Note;
                    CategoryId = _selectedTransaction.CategoryId;
                    WalletId = _selectedTransaction.WalletId;
                    Type = _selectedTransaction.Type;
                }
                else
                {
                    HeaderTitle = "quick add";
                    AmountText = string.Empty;
                    Note = string.Empty;
                    CategoryId = 0;
                    WalletId = Wallets.Count > 0 ? Wallets[0].Id : 0;
                    Type = "Expense";
                }
                OnPropertyChanged();
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

                var wallets = await _walletService.GetWalletsAsync();
                Wallets.Clear();
                foreach (var wallet in wallets)
                {
                    Wallets.Add(wallet);
                }
                if (SelectedTransaction == null && Wallets.Count > 0 && WalletId == 0)
                {
                    WalletId = Wallets[0].Id;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "failed to load data: " + ex.Message.ToLower();
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
                ErrorMessage = "failed to load transactions: " + ex.Message.ToLower();
            }
            finally { IsBusy = false; }
        }

        public async Task<Transaction> GetTransactionByIdAsync(int id)
        {
            return await _transactionService.GetTransactionByIdAsync(id);
        }

        public async Task<bool> SaveTransactionAsync()
        {
            ErrorMessage = string.Empty;

            double amount;
            if (!double.TryParse(AmountText, out amount) || amount <= 0)
            {
                ErrorMessage = "amount must be a positive number";
                return false;
            }

            if (CategoryId == 0)
            {
                ErrorMessage = "please select a category";
                return false;
            }

            if (WalletId == 0)
            {
                ErrorMessage = "please select a wallet";
                return false;
            }

            if (string.IsNullOrEmpty(Type))
            {
                ErrorMessage = "please select a transaction type";
                return false;
            }

            if (IsBusy) return false;
            IsBusy = true;

            try
            {
                if (SelectedTransaction == null)
                {
                    var transaction = new Transaction
                    {
                        Amount = amount,
                        Note = string.IsNullOrEmpty(Note) ? string.Empty : Note.Trim().ToLower(),
                        CategoryId = CategoryId,
                        WalletId = WalletId,
                        Type = Type,
                        Date = DateTime.Now
                    };

                    await _transactionService.AddTransactionAsync(transaction);
                    DebugData = "saved successfully";
                }
                else
                {
                    SelectedTransaction.Amount = amount;
                    SelectedTransaction.Note = string.IsNullOrEmpty(Note) ? string.Empty : Note.Trim().ToLower();
                    SelectedTransaction.CategoryId = CategoryId;
                    SelectedTransaction.WalletId = WalletId;
                    SelectedTransaction.Type = Type;

                    await _transactionService.UpdateTransactionAsync(SelectedTransaction);
                    DebugData = "updated successfully";
                }
                
                DebugColor = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Green);
                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = "failed to save transaction: " + ex.Message.ToLower();
                return false;
            }
            finally { IsBusy = false; }
        }

        public async Task<bool> DeleteTransactionAsync(int id)
        {
            if (IsBusy) return false;
            IsBusy = true;
            try
            {
                await _transactionService.DeleteTransactionAsync(id);
                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = "failed to delete transaction: " + ex.Message.ToLower();
                return false;
            }
            finally { IsBusy = false; }
        }
    }
}
