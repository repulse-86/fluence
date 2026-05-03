using Fluence.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Fluence.Services
{
    public class ProfileService
    {
        public async Task SaveProfileAsync(Profile profile)
        {
            if (profile == null) return;
            try
            {
                var settings = ApplicationData.Current.LocalSettings.Values;
                settings["InitialBalance"] = profile.InitialBalance;
                settings["MonthlyIncome"] = profile.MonthlyIncome;
                settings["MonthlyLimit"] = profile.MonthlyLimit;
                settings["Payday"] = profile.Payday.ToString("O"); 
                settings["WeekStart"] = (int)profile.WeekStart;
                await Task.Yield();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving profile: {ex.Message}");
                throw; 
            }
        }

        public async Task<Profile> GetProfileAsync()
        {
            try
            {
                var settings = ApplicationData.Current.LocalSettings.Values;
                await Task.Yield();
                if (settings.ContainsKey("InitialBalance"))
                {
                    var profile = new Profile();
                    
                    if (settings["InitialBalance"] is double)
                        profile.InitialBalance = (double)settings["InitialBalance"];
                    
                    if (settings.ContainsKey("MonthlyIncome") && settings["MonthlyIncome"] is double)
                        profile.MonthlyIncome = (double)settings["MonthlyIncome"];
                    
                    if (settings.ContainsKey("MonthlyLimit") && settings["MonthlyLimit"] is double)
                        profile.MonthlyLimit = (double)settings["MonthlyLimit"];

                    if (settings.ContainsKey("Payday"))
                    {
                        DateTime payday;
                        if (DateTime.TryParse((string)settings["Payday"], out payday))
                        {
                            profile.Payday = payday;
                        }
                    }
                    if (settings.ContainsKey("WeekStart"))
                    {
                        profile.WeekStart = (DayOfWeek)(int)settings["WeekStart"];
                    }
                    return profile;
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading profile: {ex.Message}");
                return null;
            }
        }

        public async Task SeedProfileAsync()
        {
            var profile = new Profile
            {
                InitialBalance = 81000,
                MonthlyIncome = 24000,
                MonthlyLimit = 5000,
                Payday = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 28),
                WeekStart = DayOfWeek.Monday
            };
            await SaveProfileAsync(profile);
        }

        public async Task ClearProfileAsync()
        {
            var settings = ApplicationData.Current.LocalSettings.Values;
            settings.Remove("InitialBalance");
            settings.Remove("MonthlyIncome");
            settings.Remove("MonthlyLimit");
            settings.Remove("Payday");
            settings.Remove("WeekStart");
            await Task.Yield();
        }
    }
}
