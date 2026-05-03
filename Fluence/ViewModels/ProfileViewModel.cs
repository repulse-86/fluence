using Fluence.Services;
using Fluence.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Fluence.ViewModels
{
    public class ProfileViewModel : INotifyPropertyChanged
    {
        private string _currentBalanceDisplay;

        private string _monthlyIncomeText;
        private string _monthlyIncomeErrorMessage;

        private string _monthlyLimitText;
        private string _monthlyLimitErrorMessage;

        private DateTimeOffset _payday = DateTimeOffset.Now;
        private string _paydayErrorMessage;

        private bool _isMondayFirst;

        private bool _isBusy;
        private bool _isEditing;

        private readonly ProfileService _profileService = new ProfileService();
        private readonly WalletService _walletService = new WalletService();

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsEditing
        {
            get { return _isEditing; }
            set
            {
                if (_isEditing != value)
                {
                    _isEditing = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SaveButtonLabel));
                    OnPropertyChanged(nameof(SaveButtonSymbol));
                    OnPropertyChanged(nameof(ViewVisibility));
                    OnPropertyChanged(nameof(EditVisibility));
                }
            }
        }

        public Visibility ViewVisibility => IsEditing ? Visibility.Collapsed : Visibility.Visible;
        public Visibility EditVisibility => IsEditing ? Visibility.Visible : Visibility.Collapsed;

        public string SaveButtonLabel => IsEditing ? "save" : "edit";
        public Symbol SaveButtonSymbol => IsEditing ? Symbol.Save : Symbol.Edit;

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    OnPropertyChanged();
                }
            }
        }

        public string CurrentBalanceDisplay
        {
            get { return _currentBalanceDisplay; }
            set
            {
                if (_currentBalanceDisplay != value)
                {
                    _currentBalanceDisplay = value;
                    OnPropertyChanged();
                }
            }
        }

        public string MonthlyIncomeText
        {
            get { return _monthlyIncomeText; }
            set
            {
                if (_monthlyIncomeText != value)
                {
                    _monthlyIncomeText = value;
                    OnPropertyChanged();
                }
            }
        }

        public string MonthlyIncomeErrorMessage
        {
            get { return _monthlyIncomeErrorMessage; }
            set
            {
                if (_monthlyIncomeErrorMessage != value)
                {
                    _monthlyIncomeErrorMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public string MonthlyLimitText
        {
            get { return _monthlyLimitText; }
            set
            {
                if (_monthlyLimitText != value)
                {
                    _monthlyLimitText = value;
                    OnPropertyChanged();
                }
            }
        }

        public string MonthlyLimitErrorMessage
        {
            get { return _monthlyLimitErrorMessage; }
            set
            {
                if (_monthlyLimitErrorMessage != value)
                {
                    _monthlyLimitErrorMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTimeOffset Payday
        {
            get { return _payday; }
            set
            {
                if (_payday != value)
                {
                    _payday = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(PaydayFormatted));
                }
            }
        }

        public bool IsMondayFirst
        {
            get { return _isMondayFirst; }
            set
            {
                if (_isMondayFirst != value)
                {
                    _isMondayFirst = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(WeekStartFormatted));
                }
            }
        }

        public string PaydayFormatted => Payday.ToString("MMMM dd");
        public string WeekStartFormatted => IsMondayFirst ? "starts on monday" : "starts on sunday";

        public string PaydayErrorMessage
        {
            get { return _paydayErrorMessage; }
            set
            {
                if (_paydayErrorMessage != value)
                {
                    _paydayErrorMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _debugData;
        private Windows.UI.Xaml.Media.Brush _debugColor = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Gray);

        public string DebugData
        {
            get { return _debugData; }
            set
            {
                if (_debugData != value)
                {
                    _debugData = value;
                    OnPropertyChanged();
                }
            }
        }

        public Windows.UI.Xaml.Media.Brush DebugColor
        {
            get { return _debugColor; }
            set
            {
                if (_debugColor != value)
                {
                    _debugColor = value;
                    OnPropertyChanged();
                }
            }
        }

        public async Task LoadProfileDetailsAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var data = await _profileService.GetProfileAsync();
                
                var wallets = await _walletService.GetWalletsAsync();
                CurrentBalanceDisplay = wallets.Sum(w => w.Balance).ToString("N2");

                if (data != null)
                {
                    MonthlyIncomeText = data.MonthlyIncome.ToString("N2");
                    MonthlyLimitText = data.MonthlyLimit.ToString("N2");
                    Payday = data.Payday;
                    IsMondayFirst = data.WeekStart == DayOfWeek.Monday;
                    IsEditing = false;
                    DebugData = string.Empty;
                }
                else
                {
                    MonthlyIncomeText = "0.00";
                    MonthlyLimitText = "0.00";
                    Payday = DateTimeOffset.Now;
                    IsMondayFirst = true;
                    IsEditing = true;
                    DebugData = string.Empty;
                }
                
                OnPropertyChanged(string.Empty);
            }
            catch (Exception)
            {
                DebugData = "error loading profile";
                DebugColor = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Red);
            }
            finally { IsBusy = false; }
        }

        public async Task<bool> SaveProfileDetailsAsync()
        {
            MonthlyIncomeErrorMessage = string.Empty;
            MonthlyLimitErrorMessage = string.Empty;
            PaydayErrorMessage = string.Empty;
            DebugData = string.Empty;

            bool hasError = false;

            double monthlyIncome;
            if (!double.TryParse(MonthlyIncomeText, out monthlyIncome) || monthlyIncome < 0)
            {
                MonthlyIncomeErrorMessage = "Monthly income must be a positive number.";
                hasError = true;
            }

            double monthlyLimit;
            if (!double.TryParse(MonthlyLimitText, out monthlyLimit) || monthlyLimit < 0)
            {
                MonthlyLimitErrorMessage = "Monthly limit must be a positive number.";
                hasError = true;
            }

            if (Payday == DateTimeOffset.MinValue)
            {
                PaydayErrorMessage = "Please select a valid payday.";
                hasError = true;
            }

            if (hasError) return false;

            if (IsBusy) return false;
            IsBusy = true;

            try
            {
                var data = new Profile
                {
                    InitialBalance = 0, 
                    MonthlyIncome = monthlyIncome,
                    MonthlyLimit = monthlyLimit,
                    Payday = this.Payday.DateTime,
                    WeekStart = this.IsMondayFirst ? DayOfWeek.Monday : DayOfWeek.Sunday
                };
                await _profileService.SaveProfileAsync(data);
                
                DebugData = "saved successfully";
                DebugColor = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Green);
                
                return true;
            }
            catch (Exception)
            {
                DebugData = "error saving profile";
                DebugColor = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Red);
                return false;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}