using Fluence.Models;
using Fluence.ViewModels;
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace Fluence.Views.Components.Management
{
    public sealed partial class WalletManagementControl : UserControl
    {
        public WalletManagementControl()
        {
            this.InitializeComponent();
        }

        private void WalletBorder_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (e.HoldingState == Windows.UI.Input.HoldingState.Started)
            {
                FrameworkElement element = sender as FrameworkElement;
                if (element != null)
                {
                    FlyoutBase.ShowAttachedFlyout(element);
                }
            }
        }

        private void EditWallet_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuFlyoutItem;
            Wallet wallet = menuItem.DataContext as Wallet;
            var viewModel = this.DataContext as WalletViewModel;

            if (wallet != null && viewModel != null)
            {
                viewModel.SelectedWallet = wallet;
                viewModel.WalletName = wallet.Name;
                viewModel.WalletBalance = wallet.Balance.ToString();
                viewModel.IsEditing = true;
            }
        }

        private async void DeleteWallet_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuFlyoutItem;
            Wallet wallet = menuItem.DataContext as Wallet;
            var viewModel = this.DataContext as WalletViewModel;

            if (wallet != null && viewModel != null)
            {
                var dialog = new MessageDialog($"are you sure you want to delete '{wallet.Name}'?", "confirm delete");
                dialog.Commands.Add(new UICommand("delete") { Id = 0 });
                dialog.Commands.Add(new UICommand("cancel") { Id = 1 });
                dialog.DefaultCommandIndex = 1;

                var result = await dialog.ShowAsync();
                if ((int)result.Id == 0)
                {
                    await viewModel.DeleteWalletAsync(wallet.Id);
                    await viewModel.LoadWalletsAsync();
                    HubViewModel.IsOverviewDirty = true;
                }
            }
        }
    }
}
