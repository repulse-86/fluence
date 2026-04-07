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

    public class ReportCategoryItem
    {
        public string Name { get; set; }
        public double Amount { get; set; }
        public double Percentage { get; set; }
        public Brush Color { get; set; }
    }

    public class PeriodReport : INotifyPropertyChanged
    {
        private double _flowIn;
        public double FlowIn { get { return _flowIn; } set { _flowIn = value; OnPropertyChanged(); } }

        private double _flowOut;
        public double FlowOut { get { return _flowOut; } set { _flowOut = value; OnPropertyChanged(); } }

        private double _flowInPercent;
        public double FlowInPercent { get { return _flowInPercent; } set { _flowInPercent = value; OnPropertyChanged(); } }

        private double _flowOutPercent;
        public double FlowOutPercent { get { return _flowOutPercent; } set { _flowOutPercent = value; OnPropertyChanged(); } }

        public ObservableCollection<ReportCategoryItem> Distribution { get; set; } = new ObservableCollection<ReportCategoryItem>();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
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

        public PeriodReport DayReport { get; set; } = new PeriodReport();
        public PeriodReport MonthReport { get; set; } = new PeriodReport();
        public PeriodReport YearReport { get; set; } = new PeriodReport();

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

        public async Task LoadReportAsync()
        {
            var transactions = await _transactionService.GetTransactionsAsync();
            var categories = await _categoryService.GetCategoriesAsync();
            var categoryMap = categories.ToDictionary(c => c.Id, c => c.Name);

            DateTime now = DateTime.Now;

            await PopulatePeriodReport(DayReport, transactions.Where(t => t.Date.Date == now.Date), categoryMap);
            await PopulatePeriodReport(MonthReport, transactions.Where(t => t.Date.Month == now.Month && t.Date.Year == now.Year), categoryMap);
            await PopulatePeriodReport(YearReport, transactions.Where(t => t.Date.Year == now.Year), categoryMap);
        }

        private async Task PopulatePeriodReport(PeriodReport report, IEnumerable<Transaction> filtered, Dictionary<int, string> categoryMap)
        {
            report.FlowIn = filtered.Where(t => t.Type == "Income").Sum(t => t.Amount);
            report.FlowOut = filtered.Where(t => t.Type == "Expense").Sum(t => t.Amount);

            double totalFlow = report.FlowIn + report.FlowOut;
            if (totalFlow > 0)
            {
                report.FlowInPercent = (report.FlowIn / totalFlow) * 100;
                report.FlowOutPercent = (report.FlowOut / totalFlow) * 100;
            }
            else { report.FlowInPercent = report.FlowOutPercent = 0; }

            report.Distribution.Clear();
            var expenses = filtered.Where(t => t.Type == "Expense").ToList();
            if (expenses.Any())
            {
                double totalExpenses = expenses.Sum(t => t.Amount);
                var grouped = expenses.GroupBy(t => t.CategoryId)
                    .OrderByDescending(g => g.Sum(t => t.Amount));

                Color[] palette = {
                    Color.FromArgb(255, 52, 152, 219), Color.FromArgb(255, 155, 89, 182),
                    Color.FromArgb(255, 26, 188, 156), Color.FromArgb(255, 241, 196, 15),
                    Color.FromArgb(255, 230, 126, 34), Color.FromArgb(255, 231, 76, 60)
                };

                int i = 0;
                foreach (var g in grouped)
                {
                    double amt = g.Sum(t => t.Amount);
                    report.Distribution.Add(new ReportCategoryItem
                    {
                        Name = categoryMap.ContainsKey(g.Key) ? categoryMap[g.Key].ToLower() : "unknown",
                        Amount = amt,
                        Percentage = (amt / totalExpenses) * 100,
                        Color = new SolidColorBrush(palette[i % palette.Length])
                    });
                    i++;
                }
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
