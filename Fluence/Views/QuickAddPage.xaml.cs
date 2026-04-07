using Fluence.ViewModels;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Fluence.Views
{
    public sealed partial class QuickAddPage : Page
    {
        private TransactionViewModel _viewModel;

        public QuickAddPage()
        {
            this.InitializeComponent();
            _viewModel = new TransactionViewModel();
            this.DataContext = _viewModel;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
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

        private async void SaveTransactionAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (await _viewModel.SaveTransactionAsync())
            {
                if (Frame.CanGoBack)
                {
                    Frame.GoBack();
                }
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
