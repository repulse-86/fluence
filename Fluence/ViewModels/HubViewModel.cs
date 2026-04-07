using Fluence.Models;
using Fluence.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Fluence.ViewModels
{
    public class TransactionDisplayItem : INotifyPropertyChanged
    {
        private bool _isExpanded;
        public string CategoryName { get; set; }
        public string Note { get; set; }
        public string Type { get; set; }
        public double Amount { get; set; }
        public string DisplayAmount { get; set; }
        public string DisplayDate { get; set; }
        public string DisplayTime { get; set; }
        public Brush TypeBrush { get; set; }

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                _isExpanded = value;
                OnPropertyChanged();
                OnPropertyChanged("NoteVisibility");
            }
        }

        public Visibility NoteVisibility
        {
            get { return _isExpanded ? Visibility.Visible : Visibility.Collapsed; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class TransactionGroup : ObservableCollection<TransactionDisplayItem>
    {
        public string DateKey { get; set; }
    }

    public class HubViewModel : INotifyPropertyChanged
    {
        private readonly TransactionService _transactionService = new TransactionService();
        private readonly CategoryService _categoryService = new CategoryService();

        public ObservableCollection<TransactionGroup> GroupedTransactions { get; set; } = new ObservableCollection<TransactionGroup>();
        public ObservableCollection<TransactionDisplayItem> Transactions { get; set; } = new ObservableCollection<TransactionDisplayItem>();

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task LoadHistoryAsync()
        {
            var transactions = await _transactionService.GetTransactionsAsync();
            var categories = await _categoryService.GetCategoriesAsync();
            var categoryMap = new Dictionary<int, string>();
            foreach (var category in categories)
            {
                categoryMap[category.Id] = category.Name;
            }

            Transactions.Clear();
            GroupedTransactions.Clear();

            TransactionGroup currentGroup = null;
            string currentDateKey = null;

            foreach (var t in transactions)
            {
                string categoryName = categoryMap.ContainsKey(t.CategoryId) ? categoryMap[t.CategoryId] : "unknown";
                string dateKey = t.Date.ToString("MMMM d, yyyy").ToLower();

                var displayItem = new TransactionDisplayItem
                {
                    CategoryName = categoryName.ToLower(),
                    Note = t.Note?.ToLower(),
                    Type = t.Type,
                    Amount = t.Amount,
                    DisplayAmount = (t.Type == "Income" ? "+" : "-") + t.Amount.ToString("N2"),
                    DisplayDate = t.Date.ToString("MMM dd").ToLower(),
                    DisplayTime = t.Date.ToString("t").ToLower(),
                    TypeBrush = t.Type == "Income" ? new SolidColorBrush(Color.FromArgb(255, 46, 204, 113)) : new SolidColorBrush(Color.FromArgb(255, 231, 76, 60)),
                    IsExpanded = false
                };

                Transactions.Add(displayItem);

                if (currentDateKey != dateKey)
                {
                    currentDateKey = dateKey;
                    currentGroup = new TransactionGroup { DateKey = dateKey };
                    GroupedTransactions.Add(currentGroup);
                }

                currentGroup.Add(displayItem);
            }
        }
    }
}
