using Fluence.Common;
using Fluence.ViewModels;
using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Fluence.Services;
using System.Threading.Tasks;

namespace Fluence.Views.Transactions
{
    public sealed partial class TransactionDetailsPage : Page
    {
        private readonly NavigationHelper navigationHelper;
        private TransactionDisplayItem _displayItem;
        private int _transactionId;

        public TransactionDetailsPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        public NavigationHelper NavigationHelper => this.navigationHelper;

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e) { }
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e) { }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
            if (e.Parameter is int)
            {
                _transactionId = (int)e.Parameter;
                await LoadTransactionDetailsAsync();
            }
        }

        private async Task LoadTransactionDetailsAsync()
        {
            var transactionViewModel = new TransactionViewModel();
            var t = await transactionViewModel.GetTransactionByIdAsync(_transactionId);
            if (t != null)
            {
                var categoryService = new CategoryService();
                var cat = await categoryService.GetCategoryByIdAsync(t.CategoryId);

                var walletService = new WalletService();
                var wallet = await walletService.GetWalletByIdAsync(t.WalletId);

                _displayItem = new TransactionDisplayItem
                {
                    Id = t.Id,
                    CategoryName = cat?.Name ?? "unknown",
                    WalletName = wallet?.Name ?? "unknown",
                    Note = t.Note,
                    Type = t.Type,
                    Amount = t.Amount,
                    DisplayAmount = (t.Type == "Income" ? "+" : "-") + t.Amount.ToString("N2"),
                    DisplayDate = t.Date.ToString("MMMM dd, yyyy").ToLower(),
                    DisplayTime = t.Date.ToString("t").ToLower(),
                    TypeBrush = t.Type == "Income" ? new SolidColorBrush(Color.FromArgb(255, 46, 204, 113)) : new SolidColorBrush(Color.FromArgb(255, 231, 76, 60))
                };
                this.DataContext = _displayItem;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        private void EditAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(QuickAddPage), _transactionId);
        }

        private async void DeleteAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Windows.UI.Popups.MessageDialog("are you sure you want to delete this transaction?", "confirm delete");
            dialog.Commands.Add(new Windows.UI.Popups.UICommand("delete") { Id = 0 });
            dialog.Commands.Add(new Windows.UI.Popups.UICommand("cancel") { Id = 1 });
            dialog.DefaultCommandIndex = 1;

            var result = await dialog.ShowAsync();
            if ((int)result.Id == 0)
            {
                var transactionViewModel = new TransactionViewModel();
                if (await transactionViewModel.DeleteTransactionAsync(_transactionId))
                {
                    HubViewModel.IsDirty = true;
                    if (Frame.CanGoBack) Frame.GoBack();
                }
            }
        }
    }
}
