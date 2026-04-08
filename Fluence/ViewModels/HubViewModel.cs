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
        public int Id { get; set; }
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
                OnPropertyChanged("NoteWrapping");
                OnPropertyChanged("NoteTrimming");
            }
        }

        public Visibility NoteVisibility
        {
            get
            {
                if (string.IsNullOrEmpty(Note) || !_isExpanded)
                    return Visibility.Collapsed;

                return Visibility.Visible;
            }
        }

        public TextWrapping NoteWrapping
        {
            get { return _isExpanded ? TextWrapping.Wrap : TextWrapping.NoWrap; }
        }

        public TextTrimming NoteTrimming
        {
            get { return _isExpanded ? TextTrimming.None : TextTrimming.CharacterEllipsis; }
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
        public int TransactionCount { get; set; }
        public double Trend { get; set; } 
        public Brush Color { get; set; }
    }

    public class IntensityItem
    {
        public double Intensity { get; set; } 
        public string Label { get; set; }
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

        private double _savingsRate;
        public double SavingsRate { get { return _savingsRate; } set { _savingsRate = value; OnPropertyChanged(); } }

        private double _trendPercent;
        public double TrendPercent { get { return _trendPercent; } set { _trendPercent = value; OnPropertyChanged(); } }

        private string _trendDirection = "none";
        public string TrendDirection { get { return _trendDirection; } set { _trendDirection = value; OnPropertyChanged(); } }

        private double _dailyAverage;
        public double DailyAverage { get { return _dailyAverage; } set { _dailyAverage = value; OnPropertyChanged(); } }

        private double _projectedTotal;
        public double ProjectedTotal { get { return _projectedTotal; } set { _projectedTotal = value; OnPropertyChanged(); } }

        private ObservableCollection<ReportCategoryItem> _distribution = new ObservableCollection<ReportCategoryItem>();
        public ObservableCollection<ReportCategoryItem> Distribution
        {
            get { return _distribution; }
            set { _distribution = value; OnPropertyChanged(); }
        }

        private ObservableCollection<ReportCategoryItem> _budgetBurners = new ObservableCollection<ReportCategoryItem>();
        public ObservableCollection<ReportCategoryItem> BudgetBurners
        {
            get { return _budgetBurners; }
            set { _budgetBurners = value; OnPropertyChanged(); }
        }

        private ObservableCollection<IntensityItem> _intensityMap = new ObservableCollection<IntensityItem>();
        public ObservableCollection<IntensityItem> IntensityMap
        {
            get { return _intensityMap; }
            set { _intensityMap = value; OnPropertyChanged(); }
        }

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

        public static bool IsDirty { get; set; } = true;

        private ObservableCollection<TransactionGroup> _groupedTransactions = new ObservableCollection<TransactionGroup>();
        public ObservableCollection<TransactionGroup> GroupedTransactions
        {
            get { return _groupedTransactions; }
            set { _groupedTransactions = value; OnPropertyChanged(); }
        }
        
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

        private int _historySkip = 0;
        private const int HistoryTake = 10;
        private bool _isLoadingHistory = false;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task LoadOverviewAsync()
        {
            var transactions = await _transactionService.GetTransactionsAsync();
            var profile = await _profileService.GetProfileAsync();
            
            BudgetPercent = 0;
            DailyAllowance = 0;

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
                DailyAllowance = Math.Round(Math.Max(0, (profile.MonthlyLimit - monthlySpent) / daysRemaining), 2);
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

            DateTime today = now.Date;
            DateTime yesterday = today.AddDays(-1);
            PopulatePeriodReport(DayReport,
                transactions.Where(t => t.Date.Date == today), 
                transactions.Where(t => t.Date.Date == yesterday),
                categoryMap, "day");

            await Task.Delay(50);

            DateTime startOfMonth = new DateTime(now.Year, now.Month, 1);
            DateTime startOfLastMonth = startOfMonth.AddMonths(-1);
            DateTime sameDayLastMonth = now.AddMonths(-1);

            PopulatePeriodReport(MonthReport,
                transactions.Where(t => t.Date >= startOfMonth && t.Date <= now),
                transactions.Where(t => t.Date >= startOfLastMonth && t.Date <= sameDayLastMonth),
                categoryMap, "month");

            await Task.Delay(50);

            DateTime startOfYear = new DateTime(now.Year, 1, 1);
            DateTime startOfLastYear = new DateTime(now.Year - 1, 1, 1);
            DateTime sameDayLastYear = now.AddYears(-1);

            PopulatePeriodReport(YearReport,
                transactions.Where(t => t.Date >= startOfYear && t.Date <= now),
                transactions.Where(t => t.Date >= startOfLastYear && t.Date <= sameDayLastYear),
                categoryMap, "year");
        }

        private void PopulatePeriodReport(PeriodReport report, IEnumerable<Transaction> current, IEnumerable<Transaction> previous, Dictionary<int, string> categoryMap, string type)
        {
            var currentList = current.ToList();
            var previousList = previous.ToList();

            report.FlowIn = currentList.Where(t => t.Type == "Income").Sum(t => t.Amount);
            report.FlowOut = currentList.Where(t => t.Type == "Expense").Sum(t => t.Amount);

            double totalFlow = report.FlowIn + report.FlowOut;
            if (totalFlow > 0)
            {
                report.FlowInPercent = (report.FlowIn / totalFlow) * 100;
                report.FlowOutPercent = (report.FlowOut / totalFlow) * 100;
            }
            else { report.FlowInPercent = report.FlowOutPercent = 0; }

            if (report.FlowIn > 0)
            {
                report.SavingsRate = Math.Max(0, (report.FlowIn - report.FlowOut) / report.FlowIn * 100);
            }
            else { report.SavingsRate = 0; }

            double prevOut = previousList.Where(t => t.Type == "Expense").Sum(t => t.Amount);
            if (prevOut > 0)
            {
                report.TrendPercent = Math.Abs((report.FlowOut - prevOut) / prevOut * 100);
                report.TrendDirection = report.FlowOut > prevOut ? "up" : (report.FlowOut < prevOut ? "down" : "same");
            }
            else 
            { 
                report.TrendPercent = report.FlowOut > 0 ? 100 : 0; 
                report.TrendDirection = report.FlowOut > 0 ? "up" : "none"; 
            }

            DateTime now = DateTime.Now;
            if (type == "month")
            {
                int daysPassed = now.Day;
                int daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
                report.DailyAverage = report.FlowOut / daysPassed;
                report.ProjectedTotal = report.DailyAverage * daysInMonth;
            }
            else if (type == "year")
            {
                int monthsPassed = now.Month;
                report.DailyAverage = report.FlowOut / monthsPassed; 
                report.ProjectedTotal = report.DailyAverage * 12;
            }

            var newIntensityMap = new ObservableCollection<IntensityItem>();
            if (type == "month")
            {
                var dailySpending = currentList.Where(t => t.Type == "Expense")
                    .GroupBy(t => t.Date.Day)
                    .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));
                
                double maxDaily = dailySpending.Values.Count > 0 ? dailySpending.Values.Max() : 0;
                int daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
                
                for (int d = 1; d <= daysInMonth; d++)
                {
                    double amt = dailySpending.ContainsKey(d) ? dailySpending[d] : 0;
                    newIntensityMap.Add(new IntensityItem {
                        Intensity = maxDaily > 0 ? Math.Max(0.1, amt / maxDaily) : 0.05,
                        Label = d.ToString()
                    });
                }
            }
            else if (type == "year")
            {
                var monthlySpending = currentList.Where(t => t.Type == "Expense")
                    .GroupBy(t => t.Date.Month)
                    .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

                double maxMonthly = monthlySpending.Values.Count > 0 ? monthlySpending.Values.Max() : 0;
                for (int m = 1; m <= 12; m++)
                {
                    double amt = monthlySpending.ContainsKey(m) ? monthlySpending[m] : 0;
                    newIntensityMap.Add(new IntensityItem {
                        Intensity = maxMonthly > 0 ? Math.Max(0.1, amt / maxMonthly) : 0.05,
                        Label = new DateTime(2000, m, 1).ToString("MMM").ToLower().Substring(0, 1)
                    });
                }
            }
            report.IntensityMap = newIntensityMap;

            var newDistribution = new ObservableCollection<ReportCategoryItem>();
            var newBudgetBurners = new ObservableCollection<ReportCategoryItem>();
            var currentExpenses = currentList.Where(t => t.Type == "Expense").ToList();
            var prevExpenses = previousList.Where(t => t.Type == "Expense").ToList();

            if (currentExpenses.Any())
            {
                double totalExpenses = currentExpenses.Sum(t => t.Amount);
                var grouped = currentExpenses.GroupBy(t => t.CategoryId)
                    .OrderByDescending(g => g.Sum(t => t.Amount));

                var prevGrouped = prevExpenses.GroupBy(t => t.CategoryId)
                    .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

                Color[] palette = {
                    Color.FromArgb(255, 52, 152, 219), Color.FromArgb(255, 155, 89, 182),
                    Color.FromArgb(255, 26, 188, 156), Color.FromArgb(255, 241, 196, 15),
                    Color.FromArgb(255, 230, 126, 34), Color.FromArgb(255, 231, 76, 60)
                };

                int i = 0;
                var allItems = new List<ReportCategoryItem>();
                foreach (var g in grouped)
                {
                    double amt = g.Sum(t => t.Amount);
                    double prevAmt = prevGrouped.ContainsKey(g.Key) ? prevGrouped[g.Key] : 0;
                    
                    double catTrend = prevAmt > 0 ? (amt - prevAmt) / prevAmt * 100 : 100;

                    var item = new ReportCategoryItem
                    {
                        Name = categoryMap.ContainsKey(g.Key) ? categoryMap[g.Key].ToLower() : "unknown",
                        Amount = amt,
                        Percentage = (amt / totalExpenses) * 100,
                        TransactionCount = g.Count(),
                        Trend = catTrend,
                        Color = new SolidColorBrush(palette[i % palette.Length])
                    };
                    newDistribution.Add(item);
                    allItems.Add(item);
                    i++;
                }

                var burners = allItems.Where(x => x.Trend > 0).OrderByDescending(item => item.Trend).Take(3);
                foreach (var b in burners) newBudgetBurners.Add(b);
            }
            report.Distribution = newDistribution;
            report.BudgetBurners = newBudgetBurners;
        }

        public async Task LoadHistoryAsync()
        {
            _historySkip = 0;
            GroupedTransactions = new ObservableCollection<TransactionGroup>();
            await LoadMoreHistoryAsync();
        }

        public async Task LoadMoreHistoryAsync()
        {
            if (_isLoadingHistory) return;
            _isLoadingHistory = true;

            try
            {
                var transactions = await _transactionService.GetTransactionsAsync(_historySkip, HistoryTake);
                if (transactions.Count == 0) return;

                var categories = await _categoryService.GetCategoriesAsync();
                var categoryMap = categories.ToDictionary(c => c.Id, c => c.Name);

                TransactionGroup currentGroup = GroupedTransactions.LastOrDefault();

                foreach (var t in transactions)
                {
                    string categoryName = categoryMap.ContainsKey(t.CategoryId) ? categoryMap[t.CategoryId] : "unknown";
                    string dateKey = t.Date.ToString("MMMM d, yyyy").ToLower();

                    var displayItem = new TransactionDisplayItem
                    {
                        Id = t.Id,
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

                    bool isNewGroup = false;
                    if (currentGroup == null || currentGroup.DateKey != dateKey)
                    {
                        currentGroup = new TransactionGroup { DateKey = dateKey };
                        isNewGroup = true;
                    }

                    currentGroup.Add(displayItem);

                    if (isNewGroup)
                    {
                        GroupedTransactions.Add(currentGroup);
                        await Task.Delay(30);
                    }
                }

                _historySkip += transactions.Count;
            }
            finally
            {
                _isLoadingHistory = false;
            }
        }
    }
}
