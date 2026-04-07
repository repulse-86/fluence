using Fluence.Models;
using Fluence.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
        private readonly ProfileService _profileService = new ProfileService();

        public ObservableCollection<TransactionGroup> GroupedTransactions { get; set; } = new ObservableCollection<TransactionGroup>();
        
        private double _currentBalance;
        public double CurrentBalance
        {
            get { return _currentBalance; }
            set { _currentBalance = value; OnPropertyChanged(); }
        }

        private double _budgetPercent;
        public double BudgetPercent
        {
            get { return _budgetPercent; }
            set { _budgetPercent = value; OnPropertyChanged(); OnPropertyChanged("BudgetBrush"); }
        }

        public Brush BudgetBrush
        {
            get
            {
                if (_budgetPercent < 50) return new SolidColorBrush(Color.FromArgb(255, 46, 204, 113));
                if (_budgetPercent < 80) return new SolidColorBrush(Color.FromArgb(255, 241, 196, 15));
                return new SolidColorBrush(Color.FromArgb(255, 231, 76, 60));
            }
        }

        private double _dailyAllowance;
        public double DailyAllowance
        {
            get { return _dailyAllowance; }
            set { _dailyAllowance = value; OnPropertyChanged(); }
        }

        private double _spentToday;
        public double SpentToday
        {
            get { return _spentToday; }
            set { _spentToday = value; OnPropertyChanged(); }
        }

        private double _weeklyTotal;
        public double WeeklyTotal
        {
            get { return _weeklyTotal; }
            set { _weeklyTotal = value; OnPropertyChanged(); }
        }

        private string _topCategory = "none";
        public string TopCategory
        {
            get { return _topCategory; }
            set { _topCategory = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task LoadOverviewAsync()
        {
            var transactions = await _transactionService.GetTransactionsAsync();
            var profile = await _profileService.GetProfileAsync();
            
            double totalIncome = transactions.Where(t => t.Type == "Income").Sum(t => t.Amount);
            double totalExpense = transactions.Where(t => t.Type == "Expense").Sum(t => t.Amount);
            CurrentBalance = totalIncome - totalExpense;

            if (profile != null && profile.MonthlyLimit > 0)
            {
                double monthlySpent = transactions
                    .Where(t => t.Type == "Expense" && t.Date.Month == DateTime.Now.Month && t.Date.Year == DateTime.Now.Year)
                    .Sum(t => t.Amount);
                BudgetPercent = Math.Min(100, (monthlySpent / profile.MonthlyLimit) * 100);

                int daysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
                int daysRemaining = daysInMonth - DateTime.Now.Day + 1;
                DailyAllowance = Math.Max(0, (profile.MonthlyLimit - monthlySpent) / daysRemaining);
            }

            DateTime today = DateTime.Today;
            SpentToday = transactions
                .Where(t => t.Type == "Expense" && t.Date.Date == today)
                .Sum(t => t.Amount);

            DateTime startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            WeeklyTotal = transactions
                .Where(t => t.Type == "Expense" && t.Date.Date >= startOfWeek)
                .Sum(t => t.Amount);

            var topCat = transactions
                .Where(t => t.Type == "Expense")
                .GroupBy(t => t.CategoryId)
                .OrderByDescending(g => g.Sum(t => t.Amount))
                .FirstOrDefault();

            if (topCat != null)
            {
                var cat = await _categoryService.GetCategoryByIdAsync(topCat.Key);
                TopCategory = cat?.Name?.ToLower() ?? "unknown";
            }
        }

        public async Task LoadHistoryAsync()
        {
            var transactions = await _transactionService.GetTransactionsAsync();
            var categories = await _categoryService.GetCategoriesAsync();
            var categoryMap = categories.ToDictionary(c => c.Id, c => c.Name);

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
