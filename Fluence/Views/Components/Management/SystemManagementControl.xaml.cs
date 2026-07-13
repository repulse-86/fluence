using Fluence.Services;
using Fluence.ViewModels;
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Fluence.Views.Components.Management
{
    public sealed partial class SystemManagementControl : UserControl
    {
        public event EventHandler DataChanged;

        public SystemManagementControl()
        {
            this.InitializeComponent();
        }

        private void AutoRefreshWallpaper_Toggled(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as SystemViewModel;
            if (viewModel != null)
            {
                viewModel.SaveSettings();
            }
        }

        private async void MockData_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new MessageDialog("this will PERMANENTLY delete all your current transactions and replace them with 30 days of mock data. proceed?", "confirm mock data");
            dialog.Commands.Add(new UICommand("proceed") { Id = 0 });
            dialog.Commands.Add(new UICommand("cancel") { Id = 1 });
            dialog.DefaultCommandIndex = 1;

            var result = await dialog.ShowAsync();
            if ((int)result.Id == 0)
            {
                var categoryService = new CategoryService();
                await categoryService.InitializeDatabaseSync();

                var limitsService = new LimitsService();
                await limitsService.SeedLimitsAsync();

                var transactionService = new TransactionService();
                await transactionService.SeedTransactionsAsync();

                HubViewModel.IsDirty = true;
                HubViewModel.IsOverviewDirty = true;
                HubViewModel.IsHistoryDirty = true;
                HubViewModel.IsReportsDirty = true;

                DataChanged?.Invoke(this, EventArgs.Empty);

                await new MessageDialog("mock data activated successfully.", "success").ShowAsync();
            }
        }

        private async void ClearData_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new MessageDialog("this will PERMANENTLY delete ALL-TIME transactions and reset your limits settings. this cannot be undone. proceed?", "DANGER: CLEAR ALL DATA");
            dialog.Commands.Add(new UICommand("clear everything") { Id = 0 });
            dialog.Commands.Add(new UICommand("cancel") { Id = 1 });
            dialog.DefaultCommandIndex = 1;

            var result = await dialog.ShowAsync();
            if ((int)result.Id == 0)
            {
                var transactionService = new TransactionService();
                await transactionService.ClearTransactionsAsync();

                var limitsService = new LimitsService();
                await limitsService.ClearLimitsAsync();

                HubViewModel.IsDirty = true;

                DataChanged?.Invoke(this, EventArgs.Empty);

                await new MessageDialog("all data cleared successfully.", "success").ShowAsync();
            }
        }
    }
}
