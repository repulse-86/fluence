using Fluence.Common;
using Fluence.Models;
using Fluence.Services;
using Fluence.ViewModels;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Fluence.Views
{
    public sealed partial class WalletPage : Page
    {
        private readonly WalletService _walletService = new WalletService();
        private readonly WalletViewModel _walletViewModel = new WalletViewModel();
        private readonly NavigationHelper navigationHelper;
        private int _editingWalletId;

        public WalletPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);

            if (e.Parameter is int)
            {
                _editingWalletId = (int)e.Parameter;
                var wallet = await _walletService.GetWalletByIdAsync(_editingWalletId);
                if (wallet != null)
                {
                    WalletNameTextBox.Text = wallet.Name;
                    WalletBalanceTextBox.Text = wallet.Balance.ToString();
                    TitleText.Text = "edit wallet";
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        private async void SaveAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            string name = WalletNameTextBox.Text;
            string balanceText = WalletBalanceTextBox.Text;

            if (string.IsNullOrWhiteSpace(name))
            {
                ErrorText.Text = "wallet name cannot be empty.";
                return;
            }

            double balance;
            if (!double.TryParse(balanceText, out balance))
            {
                balance = 0;
            }

            ErrorText.Text = "";

            if (_editingWalletId > 0)
            {
                var wallet = await _walletService.GetWalletByIdAsync(_editingWalletId);
                if (wallet != null)
                {
                    wallet.Name = name;
                    wallet.Balance = balance;
                    await _walletViewModel.SaveWalletAsync(wallet);
                }
            }
            else
            {
                var wallet = new Wallet { Name = name, Balance = balance };
                await _walletViewModel.SaveWalletAsync(wallet);
            }

            HubViewModel.IsOverviewDirty = true;

            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private void CancelAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }
    }
}
