using Fluence.Common;
using Fluence.ViewModels;
using System;
using System.Linq;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace Fluence.Views
{
    public sealed partial class QuickAddPage : Page
    {
        private TransactionViewModel _viewModel;
        private readonly NavigationHelper navigationHelper;

        public QuickAddPage()
        {
            this.InitializeComponent();
            _viewModel = new TransactionViewModel();
            this.DataContext = _viewModel;

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
            await _viewModel.LoadCategoriesAsync();

            if (e.Parameter is int)
            {
                int transactionId = (int)e.Parameter;
                var transaction = await _viewModel.GetTransactionByIdAsync(transactionId);
                if (transaction != null)
                {
                    _viewModel.SelectedTransaction = transaction;
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        private async void SaveTransactionAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (await _viewModel.SaveTransactionAsync())
            {
                HubViewModel.IsOverviewDirty = true;
                HubViewModel.IsHistoryDirty = true;
                HubViewModel.IsReportsDirty = true;
                
                var dialog = new MessageDialog(_viewModel.DebugData);
                await dialog.ShowAsync();
                
                _viewModel.SelectedTransaction = null;
            }
        }

        private void CancelAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private void AmountTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            bool isDigit = e.Key >= Windows.System.VirtualKey.Number0 && e.Key <= Windows.System.VirtualKey.Number9;
            bool isPadDigit = e.Key >= Windows.System.VirtualKey.NumberPad0 && e.Key <= Windows.System.VirtualKey.NumberPad9;
            bool isDecimal = e.Key == Windows.System.VirtualKey.Decimal || (int)e.Key == 190;

            if (!isDigit && !isPadDigit && !isDecimal && e.Key != Windows.System.VirtualKey.Back)
            {
                e.Handled = true;
            }

            if (isDecimal && (sender as TextBox).Text.Contains("."))
            {
                e.Handled = true;
            }
        }

        private void AmountTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            string text = tb.Text;
            string filtered = new string(text.Where(c => char.IsDigit(c) || c == '.').ToArray());

            if (text != filtered)
            {
                tb.Text = filtered;
                tb.SelectionStart = tb.Text.Length;
            }
        }
    }
}
