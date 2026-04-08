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
            try
            {
                var settings = ApplicationData.Current.LocalSettings.Values;
                settings["InitialBalance"] = profile.InitialBalance;
                settings["MonthlyIncome"] = profile.MonthlyIncome;
                settings["MonthlyLimit"] = profile.MonthlyLimit;
                settings["Payday"] = profile.Payday.ToString("O"); // ISO 8601 format
                await Task.Yield();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving profile: {ex.Message}");
                throw; // Rethrow to let ViewModel handle it
            }
        }

        public async Task<Profile> GetProfileAsync()
        {
            try
            {
                var settings = ApplicationData.Current.LocalSettings.Values;
                if (settings.ContainsKey("InitialBalance"))
                {
                    var profile = new Profile
                    {
                        InitialBalance = (double)settings["InitialBalance"],
                        MonthlyIncome = (double)settings["MonthlyIncome"],
                        MonthlyLimit = (double)settings["MonthlyLimit"]
                    };

                    if (settings.ContainsKey("Payday"))
                    {
                        DateTime payday;
                        if (DateTime.TryParse((string)settings["Payday"], out payday))
                        {
                            profile.Payday = payday;
                        }
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
                InitialBalance = 5000,
                MonthlyIncome = 3000,
                MonthlyLimit = 2000,
                Payday = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1)
            };
            await SaveProfileAsync(profile);
        }
    }
}
