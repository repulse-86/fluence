using System;
using Windows.UI;

namespace Fluence.Services
{
    public class CycleDates
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }

    public static class AppHelper
    {
        public static double CalculateDailyAllowance(double monthlyLimit, double monthlySpent, int daysRemaining)
        {
            if (monthlyLimit <= 0 || daysRemaining <= 0) return 0;
            return Math.Round(Math.Max(0, (monthlyLimit - monthlySpent) / daysRemaining), 2);
        }

        public static CycleDates GetCycleDates(DateTime now, int paydayDay)
        {
            DateTime nextPayday;
            try
            {
                int daysInThisMonth = DateTime.DaysInMonth(now.Year, now.Month);
                int day = Math.Min(paydayDay, daysInThisMonth);
                nextPayday = new DateTime(now.Year, now.Month, day);
                
                if (now.Day >= day)
                {
                    DateTime nextMonth = now.AddMonths(1);
                    int daysInNextMonth = DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month);
                    nextPayday = new DateTime(nextMonth.Year, nextMonth.Month, Math.Min(paydayDay, daysInNextMonth));
                }
            }
            catch
            {
                nextPayday = now.AddDays(30);
            }

            DateTime lastPayday;
            try
            {
                DateTime lastMonth = nextPayday.AddMonths(-1);
                int daysInLastMonth = DateTime.DaysInMonth(lastMonth.Year, lastMonth.Month);
                lastPayday = new DateTime(lastMonth.Year, lastMonth.Month, Math.Min(paydayDay, daysInLastMonth));
            }
            catch
            {
                lastPayday = nextPayday.AddDays(-30);
            }

            return new CycleDates { Start = lastPayday.Date, End = nextPayday.Date };
        }

        public static int CalculateRunwayDays(double currentBalance, double dailyBurnRate)
        {
            if (dailyBurnRate <= 0) return AppConstants.MaxRunwayDays;
            return (int)Math.Max(0, currentBalance / dailyBurnRate);
        }

        public static Color ColorFromHsl(double h, double s, double l)
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

        private static double HueToRgb(double p, double q, double t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1.0 / 6.0) return p + (q - p) * 6 * t;
            if (t < 1.0 / 2.0) return q;
            if (t < 2.0 / 3.0) return p + (q - p) * (2.0 / 3.0 - t) * 6;
            return p;
        }

        public static string GetTrendDirection(double current, double previous)
        {
            if (previous <= 0) return current > 0 ? "up" : "none";
            if (Math.Abs(current - previous) < 0.01) return "same";
            return current > previous ? "up" : "down";
        }

        public static double CalculateTrendPercent(double current, double previous)
        {
            if (previous <= 0) return current > 0 ? 100 : 0;
            return Math.Abs((current - previous) / previous * 100);
        }
    }
}
