using Windows.UI;

namespace Fluence.Services
{
    public static class AppConstants
    {
        public const double BudgetHealthThresholdSafe = 50.0;
        public const double BudgetHealthThresholdWarning = 80.0;

        public const int MaxRunwayDays = -1;
        public const int TrailingDaysForRunway = 30;

        public static readonly Color IncomeColor = Color.FromArgb(255, 46, 204, 113); // #2ECC71
        public static readonly Color ExpenseColor = Color.FromArgb(255, 231, 76, 60); // #E74C3C
        public static readonly Color WarningColor = Color.FromArgb(255, 241, 196, 15); // #F1C40F

        public const double DefaultSaturation = 0.65;
        public const double DefaultLuminosity = 0.55;

        public const string CurrencyFormat = "N2";
        public const string DateKeyFormat = "MMMM d, yyyy";
        public const string ShortDateFormat = "MMM dd";
    }
}
