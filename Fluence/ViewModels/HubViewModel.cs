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

    public class SavingOpportunity
    {
        public string Title { get; set; }
        public string Advice { get; set; }
    }

    public class WaterfallItem
    {
        public string Label { get; set; }
        public double Amount { get; set; }
        public Brush Color { get; set; }
        public double Percentage { get; set; }
    }

    public class PeriodReport : INotifyPropertyChanged
    {
        public void Clear()
        {
            Distribution.Clear();
            BudgetBurners.Clear();
            IntensityMap.Clear();
            WeekdaySpending.Clear();
            SavingOpportunities.Clear();
            WaterfallData.Clear();
            FlowIn = FlowOut = FlowInPercent = FlowOutPercent = SavingsRate = TrendPercent = DailyAverage = ProjectedTotal = RunwayDays = 0;
            TrendDirection = "none";
        }

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

        private ObservableCollection<IntensityItem> _weekdaySpending = new ObservableCollection<IntensityItem>();
        public ObservableCollection<IntensityItem> WeekdaySpending
        {
            get { return _weekdaySpending; }
            set { _weekdaySpending = value; OnPropertyChanged(); }
        }

        private int _runwayDays;
        public int RunwayDays
        {
            get { return _runwayDays; }
            set { _runwayDays = value; OnPropertyChanged(); }
        }

        private string _mostSpentDay;
        public string MostSpentDay
        {
            get { return _mostSpentDay; }
            set { _mostSpentDay = value; OnPropertyChanged(); }
        }

        private string _mostSpentMonth;
        public string MostSpentMonth
        {
            get { return _mostSpentMonth; }
            set { _mostSpentMonth = value; OnPropertyChanged(); }
        }

        private string _runwayExplanation;
        public string RunwayExplanation
        {
            get { return _runwayExplanation; }
            set { _runwayExplanation = value; OnPropertyChanged(); }
        }

        private ObservableCollection<SavingOpportunity> _savingOpportunities = new ObservableCollection<SavingOpportunity>();
        public ObservableCollection<SavingOpportunity> SavingOpportunities
        {
            get { return _savingOpportunities; }
            set { _savingOpportunities = value; OnPropertyChanged(); }
        }

        private ObservableCollection<WaterfallItem> _waterfallData = new ObservableCollection<WaterfallItem>();
        public ObservableCollection<WaterfallItem> WaterfallData
        {
            get { return _waterfallData; }
            set { _waterfallData = value; OnPropertyChanged(); }
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
        public static bool IsOverviewDirty { get; set; } = true;
        public static bool IsHistoryDirty { get; set; } = true;
        public static bool IsReportsDirty { get; set; } = true;

        private bool _isActive;
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    OnPropertyChanged();
                    OnPropertyChanged("ContentVisibility");
                    if (!_isActive) UnloadContent();
                }
            }
        }

        public Visibility ContentVisibility => _isActive ? Visibility.Visible : Visibility.Collapsed;

        private void UnloadContent()
        {
            GroupedTransactions.Clear();
            _historySkip = 0;

            DayReport.Clear();
            MonthReport.Clear();
            YearReport.Clear();

            System.Diagnostics.Debug.WriteLine("HubViewModel: UnloadContent - Data cleared");
        }

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

        private int _daysUntilPayday;
        public int DaysUntilPayday
        {
            get { return _daysUntilPayday; }
            set { _daysUntilPayday = value; OnPropertyChanged(); }
        }

        private int _runwayDays;
        public int RunwayDays
        {
            get { return _runwayDays; }
            set { _runwayDays = value; OnPropertyChanged(); }
        }

        private string _savingOpportunitiesSummary = "no opportunities found";
        public string SavingOpportunitiesSummary
        {
            get { return _savingOpportunitiesSummary; }
            set { _savingOpportunitiesSummary = value; OnPropertyChanged(); }
        }

        public string RunwayExplanation => "based on your average daily spending over the last 30 days.";

        public PeriodReport DayReport { get; set; } = new PeriodReport();
        public PeriodReport MonthReport { get; set; } = new PeriodReport();
        public PeriodReport YearReport { get; set; } = new PeriodReport();

        private int _historySkip = 0;
        private const int HistoryTake = 5;
        private bool _isLoadingHistory = false;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task LoadOverviewAsync()
        {
            System.Diagnostics.Debug.WriteLine("HubViewModel: LoadOverviewAsync - Starting");
            var profile = await _profileService.GetProfileAsync();
            
            BudgetPercent = 0;
            DailyAllowance = 0;

            double totalIncome = await _transactionService.GetTotalIncomeAsync();
            double totalExpense = await _transactionService.GetTotalExpenseAsync();
            CurrentBalance = (profile?.InitialBalance ?? 0) + totalIncome - totalExpense;

            DateTime now = DateTime.Now;
            if (profile != null && profile.MonthlyLimit > 0)
            {
                double monthlySpent = await _transactionService.GetMonthlyExpenseAsync(now.Year, now.Month);
                BudgetPercent = Math.Min(100, (monthlySpent / profile.MonthlyLimit) * 100);

                int daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
                int daysRemaining = daysInMonth - now.Day + 1;
                DailyAllowance = Math.Round(Math.Max(0, (profile.MonthlyLimit - monthlySpent) / daysRemaining), 2);
            }

            DateTime today = now.Date;
            DateTime tomorrow = today.AddDays(1);
            SpentToday = await _transactionService.GetExpenseSumAsync(today, tomorrow);

            DayOfWeek weekStart = profile?.WeekStart ?? DayOfWeek.Sunday;
            int dayOfWeek = (int)today.DayOfWeek;
            int diff;
            if (weekStart == DayOfWeek.Monday)
            {
                diff = (dayOfWeek == 0) ? 6 : dayOfWeek - 1;
            }
            else
            {
                diff = dayOfWeek;
            }

            DateTime startOfWeek = today.AddDays(-diff);
            WeeklyTotal = await _transactionService.GetExpenseSumAsync(startOfWeek, tomorrow);

            int topCatId = await _transactionService.GetTopCategoryIdAsync();
            if (topCatId > 0)
            {
                var cat = await _categoryService.GetCategoryByIdAsync(topCatId);
                TopCategory = cat?.Name ?? "unknown";
            }
            else
            {
                TopCategory = "none";
            }

            if (profile != null)
            {
                int paydayDay = profile.Payday.Day;
                DateTime nextPayday = new DateTime(now.Year, now.Month, paydayDay);
                if (now.Day > paydayDay)
                {
                    nextPayday = nextPayday.AddMonths(1);
                }
                DaysUntilPayday = (nextPayday - now.Date).Days;

                double dailyBurn = await _transactionService.GetTrailing30DayDailyBurnRateAsync();
                if (dailyBurn > 0)
                {
                    RunwayDays = (int)Math.Max(0, CurrentBalance / dailyBurn);
                }
                else RunwayDays = 999;
            }
            
            TileService.UpdateLiveTile(BudgetPercent, CurrentBalance, DailyAllowance, SpentToday, WeeklyTotal, TopCategory);
            System.Diagnostics.Debug.WriteLine("HubViewModel: LoadOverviewAsync - Completed");
        }

        public async Task LoadReportAsync()
        {
            System.Diagnostics.Debug.WriteLine("HubViewModel: LoadReportAsync - Starting");
            var categories = await _categoryService.GetCategoriesAsync();
            var categoryMap = categories.ToDictionary(c => c.Id, c => c.Name);

            DateTime now = DateTime.Now;

            DateTime today = now.Date;
            DateTime tomorrow = today.AddDays(1);
            DateTime yesterday = today.AddDays(-1);
            await PopulatePeriodReportAsync(DayReport, today, tomorrow, yesterday, today, categoryMap, "today");

            await Task.Delay(50);

            DateTime startOfMonth = new DateTime(now.Year, now.Month, 1);
            DateTime nextMonth = startOfMonth.AddMonths(1);
            DateTime startOfLastMonth = startOfMonth.AddMonths(-1);
            DateTime sameDayLastMonth = now.AddMonths(-1);
            await PopulatePeriodReportAsync(MonthReport, startOfMonth, nextMonth, startOfLastMonth, sameDayLastMonth, categoryMap, "month");

            await Task.Delay(50);

            DateTime startOfYear = new DateTime(now.Year, 1, 1);
            DateTime nextYear = startOfYear.AddYears(1);
            DateTime startOfLastYear = new DateTime(now.Year - 1, 1, 1);
            DateTime sameDayLastYear = now.AddYears(-1);
            await PopulatePeriodReportAsync(YearReport, startOfYear, nextYear, startOfLastYear, sameDayLastYear, categoryMap, "year");

            System.Diagnostics.Debug.WriteLine("HubViewModel: LoadReportAsync - Completed");
        }

        private async Task PopulatePeriodReportAsync(PeriodReport report, DateTime start, DateTime end, DateTime prevStart, DateTime prevEnd, Dictionary<int, string> categoryMap, string type)
        {
            System.Diagnostics.Debug.WriteLine($"HubViewModel: PopulatePeriodReportAsync - {type}");
            report.FlowIn = await _transactionService.GetIncomeSumAsync(start, end);
            report.FlowOut = await _transactionService.GetExpenseSumAsync(start, end);

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

            double prevOut = await _transactionService.GetExpenseSumAsync(prevStart, prevEnd);
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

                int mostSpentDayIdx = await _transactionService.GetMostSpentDayOfWeekAsync(start, end);
                if (mostSpentDayIdx >= 0)
                {
                    string[] days = { "sunday", "monday", "tuesday", "wednesday", "thursday", "friday", "saturday" };
                    report.MostSpentDay = days[mostSpentDayIdx];
                }

                int mostSpentMonthIdx = await _transactionService.GetMostSpentMonthAsync(DateTime.MinValue, DateTime.MaxValue);
                if (mostSpentMonthIdx >= 1)
                {
                    report.MostSpentMonth = new DateTime(2000, mostSpentMonthIdx, 1).ToString("MMMM").ToLower();
                }
            }
            else if (type == "year")
            {
                int monthsPassed = now.Month;
                report.DailyAverage = report.FlowOut / monthsPassed; 
                report.ProjectedTotal = report.DailyAverage * 12;
            }

            var newIntensityMap = new List<IntensityItem>();
            if (type == "month")
            {
                var currentExpenses = await _transactionService.GetTransactionsAsync(start, end);
                var dailySpending = currentExpenses.Where(t => t.Type == "Expense")
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
                var currentExpenses = await _transactionService.GetTransactionsAsync(start, end);
                var monthlySpending = currentExpenses.Where(t => t.Type == "Expense")
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
            report.IntensityMap.Clear();
            foreach (var item in newIntensityMap) report.IntensityMap.Add(item);

            var newDistribution = new List<ReportCategoryItem>();
            var newBudgetBurners = new List<ReportCategoryItem>();

            var currentSummaries = await _transactionService.GetCategorySummariesAsync(start, end);
            if (currentSummaries.Any())
            {
                var prevSummaries = await _transactionService.GetCategorySummariesAsync(prevStart, prevEnd);
                var prevGrouped = prevSummaries.ToDictionary(s => s.CategoryId, s => s.TotalAmount);

                double totalExpenses = report.FlowOut;
                var grouped = currentSummaries.OrderByDescending(s => s.TotalAmount);

                var allItems = new List<ReportCategoryItem>();
                foreach (var g in grouped)
                {
                    double amt = g.TotalAmount;
                    double prevAmt = prevGrouped.ContainsKey(g.CategoryId) ? prevGrouped[g.CategoryId] : 0;
                    double catTrend = prevAmt > 0 ? (amt - prevAmt) / prevAmt * 100 : 100;

                    allItems.Add(new ReportCategoryItem
                    {
                        Name = categoryMap.ContainsKey(g.CategoryId) ? categoryMap[g.CategoryId] : "unknown",
                        Amount = amt,
                        Percentage = (amt / totalExpenses) * 100,
                        TransactionCount = g.Count,
                        Trend = catTrend
                    });
                }

                var top10 = allItems.Take(10).ToList();
                var others = allItems.Skip(10).ToList();

                if (others.Any())
                {
                    double othersAmt = others.Sum(x => x.Amount);
                    top10.Add(new ReportCategoryItem
                    {
                        Name = "others",
                        Amount = othersAmt,
                        Percentage = (othersAmt / totalExpenses) * 100,
                        TransactionCount = others.Sum(x => x.TransactionCount),
                        Trend = 0
                    });
                }

                for (int j = 0; j < top10.Count; j++)
                {
                    double hue = (360.0 * j) / top10.Count;
                    top10[j].Color = new SolidColorBrush(ColorFromHsl(hue, 0.65, 0.55));
                    newDistribution.Add(top10[j]);
                }

                var burners = allItems.Where(x => x.Trend > 0).OrderByDescending(item => item.Trend).Take(3);
                foreach (var b in burners) newBudgetBurners.Add(b);
            }

            report.Distribution.Clear();
            foreach (var item in newDistribution) report.Distribution.Add(item);

            report.BudgetBurners.Clear();
            foreach (var item in newBudgetBurners) report.BudgetBurners.Add(item);

            var weekdays = await _transactionService.GetWeekdaySpendingAsync(start, end);
            var maxWeekday = weekdays.Any() ? weekdays.Max(x => x.TotalAmount) : 0;
            
            var profile = await _profileService.GetProfileAsync();
            DayOfWeek weekStartPreference = profile?.WeekStart ?? DayOfWeek.Sunday;
            string[] allDays = { "sun", "mon", "tue", "wed", "thu", "fri", "sat" };
            
            report.WeekdaySpending.Clear();
            for (int i = 0; i < 7; i++)
            {
                int d = ((int)weekStartPreference + i) % 7;
                var summary = weekdays.FirstOrDefault(x => x.DayOfWeek == d);
                report.WeekdaySpending.Add(new IntensityItem {
                    Label = allDays[d],
                    Intensity = maxWeekday > 0 && summary != null ? Math.Max(0.1, summary.TotalAmount / maxWeekday) : 0.05
                });
            }

            double dailyBurn = await _transactionService.GetTrailing30DayDailyBurnRateAsync();
            if (dailyBurn > 0)
            {
                report.RunwayDays = (int)Math.Max(0, CurrentBalance / dailyBurn);
            }
            else report.RunwayDays = 999;

            report.RunwayExplanation = "based on your average daily spending over the last 30 days.";

            report.SavingOpportunities.Clear();
            var highTrend = newDistribution.Where(x => x.Trend > 20).OrderByDescending(x => x.Trend).Take(3);
            foreach (var op in highTrend)
            {
                report.SavingOpportunities.Add(new SavingOpportunity {
                    Title = $"optimize {op.Name}",
                    Advice = $"spending is up {op.Trend:N0}%. reducing this could save you up to {op.Amount * 0.2:N0} per period."
                });
            }

            if (type == "month")
            {
                int count = report.SavingOpportunities.Count;
                SavingOpportunitiesSummary = count > 0 ? $"{count} saving opportunities found" : "no opportunities found";
            }

            report.WaterfallData.Clear();
            double totalWater = Math.Max(report.FlowIn, report.FlowOut);
            report.WaterfallData.Add(new WaterfallItem { Label = "income", Amount = report.FlowIn, Color = new SolidColorBrush(Color.FromArgb(255, 46, 204, 113)), Percentage = totalWater > 0 ? report.FlowIn / totalWater : 0 });
            report.WaterfallData.Add(new WaterfallItem { Label = "expense", Amount = report.FlowOut, Color = new SolidColorBrush(Color.FromArgb(255, 231, 76, 60)), Percentage = totalWater > 0 ? report.FlowOut / totalWater : 0 });
        }

        private Color ColorFromHsl(double h, double s, double l)
        {
            double r = 0, g = 0, b = 0;
            if (s == 0)
            {
                r = g = b = l;
            }
            else
            {
                double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
                double p = 2 * l - q;
                r = HueToRgb(p, q, h / 360.0 + 1.0 / 3.0);
                g = HueToRgb(p, q, h / 360.0);
                b = HueToRgb(p, q, h / 360.0 - 1.0 / 3.0);
            }
            return Color.FromArgb(255, (byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
        }

        private double HueToRgb(double p, double q, double t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1.0 / 6.0) return p + (q - p) * 6 * t;
            if (t < 1.0 / 2.0) return q;
            if (t < 2.0 / 3.0) return p + (q - p) * (2.0 / 3.0 - t) * 6;
            return p;
        }

        public async Task LoadHistoryAsync()
        {
            System.Diagnostics.Debug.WriteLine("HubViewModel: LoadHistoryAsync - Starting");

            // wait for any concurrent load to finish
            while (_isLoadingHistory) await Task.Delay(50);

            _historySkip = 0;
            GroupedTransactions.Clear();

            // initial load of 15 items to ensure the list is scrollable
            for (int i = 0; i < 3; i++)
            {
                await LoadMoreHistoryAsync();
            }

            System.Diagnostics.Debug.WriteLine("HubViewModel: LoadHistoryAsync - Completed. Groups: " + GroupedTransactions.Count);
        }

        public async Task LoadMoreHistoryAsync()
        {
            if (_isLoadingHistory) return;
            _isLoadingHistory = true;

            try
            {
                // retrieve exactly 5 items regardless of date
                var transactions = await _transactionService.GetTransactionsAsync(_historySkip, HistoryTake);
                if (transactions.Count == 0) return;

                var categories = await _categoryService.GetCategoriesAsync();
                var categoryMap = categories.ToDictionary(c => c.Id, c => c.Name);

                foreach (var t in transactions)
                {
                    string categoryName = categoryMap.ContainsKey(t.CategoryId) ? categoryMap[t.CategoryId] : "unknown";
                    string dateKey = t.Date.ToString("MMMM d, yyyy").ToLower();

                    var displayItem = new TransactionDisplayItem
                    {
                        Id = t.Id,
                        CategoryName = categoryName,
                        Note = t.Note,
                        Type = t.Type,
                        Amount = t.Amount,
                        DisplayAmount = (t.Type == "Income" ? "+" : "-") + t.Amount.ToString("N2"),
                        DisplayDate = t.Date.ToString("MMM dd").ToLower(),
                        DisplayTime = t.Date.ToString("t").ToLower(),
                        TypeBrush = t.Type == "Income" ? new SolidColorBrush(Color.FromArgb(255, 46, 204, 113)) : new SolidColorBrush(Color.FromArgb(255, 231, 76, 60)),
                        IsExpanded = false
                    };

                    var group = GroupedTransactions.FirstOrDefault(g => g.DateKey == dateKey);
                    if (group == null)
                    {
                        group = new TransactionGroup { DateKey = dateKey };
                        GroupedTransactions.Add(group);
                    }

                    // avoid duplicates if reloading
                    if (!group.Any(i => i.Id == displayItem.Id))
                    {
                        group.Add(displayItem);
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
