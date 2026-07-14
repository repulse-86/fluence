using Fluence.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Fluence.Services
{
    public class LimitsService
    {
        public async Task SaveLimitsAsync(Limits limits)
        {
            if (limits == null) return;
            try
            {
                var settings = ApplicationData.Current.LocalSettings.Values;
                settings["MonthlyIncome"] = limits.MonthlyIncome;
                settings["MonthlyLimit"] = limits.MonthlyLimit;
                settings["Payday"] = limits.Payday.ToString("O"); 
                settings["WeekStart"] = (int)limits.WeekStart;
                await Task.Yield();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving limits: {ex.Message}");
                throw; 
            }
        }

        public async Task<Limits> GetLimitsAsync()
        {
            try
            {
                var settings = ApplicationData.Current.LocalSettings.Values;
                await Task.Yield();
                if (settings.ContainsKey("MonthlyIncome"))
                {
                    var limits = new Limits();
                    
                    if (settings.ContainsKey("MonthlyIncome") && settings["MonthlyIncome"] is double)
                        limits.MonthlyIncome = (double)settings["MonthlyIncome"];
                    
                    if (settings.ContainsKey("MonthlyLimit") && settings["MonthlyLimit"] is double)
                        limits.MonthlyLimit = (double)settings["MonthlyLimit"];

                    if (settings.ContainsKey("Payday"))
                    {
                        DateTime payday;
                        if (DateTime.TryParse((string)settings["Payday"], out payday))
                        {
                            limits.Payday = payday;
                        }
                    }
                    if (settings.ContainsKey("WeekStart"))
                    {
                        limits.WeekStart = (DayOfWeek)(int)settings["WeekStart"];
                    }
                    return limits;
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading limits: {ex.Message}");
                return null;
            }
        }

        public async Task SeedLimitsAsync()
        {
            var limits = new Limits
            {
                MonthlyIncome = 24000,
                MonthlyLimit = 5000,
                Payday = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 28),
                WeekStart = DayOfWeek.Monday
            };
            await SaveLimitsAsync(limits);
        }

        public async Task ClearLimitsAsync()
        {
            var settings = ApplicationData.Current.LocalSettings.Values;
            settings.Remove("MonthlyIncome");
            settings.Remove("MonthlyLimit");
            settings.Remove("Payday");
            settings.Remove("WeekStart");
            await Task.Yield();
        }
    }
}
