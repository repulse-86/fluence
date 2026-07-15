using Fluence.Models;
using Fluence.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Fluence.ViewModels
{
    public class WalletViewModel : INotifyPropertyChanged
    {
        private readonly WalletService _walletService = new WalletService();

        private bool _isBusy;
        private string _errorMessage;
        private Wallet _selectedWallet;
        private string _walletName;
        private string _walletBalance;
        private bool _isEditing;

        public bool IsEditing
        {
            get { return _isEditing; }
            set
            {
                if (_isEditing != value)
                {
                    _isEditing = value;
                    OnPropertyChanged();
                    OnPropertyChanged("ViewVisibility");
                    OnPropertyChanged("EditVisibility");
                    OnPropertyChanged("SaveButtonLabel");
                    OnPropertyChanged("SaveButtonSymbol");
                }
            }
        }

        public Windows.UI.Xaml.Visibility ViewVisibility => IsEditing ? Windows.UI.Xaml.Visibility.Collapsed : Windows.UI.Xaml.Visibility.Visible;
        public Windows.UI.Xaml.Visibility EditVisibility => IsEditing ? Windows.UI.Xaml.Visibility.Visible : Windows.UI.Xaml.Visibility.Collapsed;
        public string SaveButtonLabel => IsEditing ? "save" : "add";
        public Symbol SaveButtonSymbol => IsEditing ? Symbol.Save : Symbol.Add;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Wallet> Wallets { get; set; } = new ObservableCollection<Wallet>();

        public string WalletName
        {
            get { return _walletName; }
            set { _walletName = value; OnPropertyChanged(); }
        }

        public string WalletBalance
        {
            get { return _walletBalance; }
            set { _walletBalance = value; OnPropertyChanged(); }
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set { if (_isBusy != value) { _isBusy = value; OnPropertyChanged(); } }
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { if (_errorMessage != value) { _errorMessage = value; OnPropertyChanged(); } }
        }

        public Wallet SelectedWallet
        {
            get { return _selectedWallet; }
            set
            {
                if (_selectedWallet != value)
                {
                    _selectedWallet = value;
                    OnPropertyChanged();
                    OnPropertyChanged("SaveButtonLabel");
                    OnPropertyChanged("SaveButtonSymbol");
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task LoadWalletsAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var wallets = await _walletService.GetWalletsAsync();
                Wallets.Clear();
                foreach (var wallet in wallets)
                {
                    Wallets.Add(wallet);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "failed to load wallets: " + ex.Message.ToLower();
            }
            finally { IsBusy = false; }
        }

        public async Task SaveWalletAsync(Wallet wallet)
        {
            if (wallet == null) return;
            try
            {
                if (wallet.Id == 0) await _walletService.AddWalletAsync(wallet);
                else await _walletService.UpdateWalletAsync(wallet);
                await LoadWalletsAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = "failed to save wallet: " + ex.Message.ToLower();
            }
        }

        public async Task SaveWalletFromFormAsync()
        {
            if (string.IsNullOrWhiteSpace(WalletName))
            {
                ErrorMessage = "wallet name is required.";
                return;
            }

            if (IsBusy) return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                double balance = 0;
                if (!string.IsNullOrWhiteSpace(WalletBalance))
                {
                    double.TryParse(WalletBalance, out balance);
                }

                if (SelectedWallet == null)
                {
                    Wallet newWallet = new Wallet { Name = WalletName.Trim(), Balance = balance };
                    await _walletService.AddWalletAsync(newWallet);
                }
                else
                {
                    SelectedWallet.Name = WalletName.Trim();
                    SelectedWallet.Balance = balance;
                    await _walletService.UpdateWalletAsync(SelectedWallet);
                    SelectedWallet = null;
                }

                WalletName = string.Empty;
                WalletBalance = string.Empty;
                IsEditing = false;
                await LoadWalletsAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = "failed to save: " + ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task DeleteWalletAsync(int walletId)
        {
            try
            {
                await _walletService.DeleteWalletAsync(walletId);
                await LoadWalletsAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = "failed to delete wallet: " + ex.Message.ToLower();
            }
        }
    }
}
