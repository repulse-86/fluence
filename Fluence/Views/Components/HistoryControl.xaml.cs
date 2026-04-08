using Fluence.ViewModels;
using Fluence.Views;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace Fluence.Views.Components
{
    public sealed partial class HistoryControl : UserControl
    {
        public event EventHandler TransactionDeleted;

        public HistoryControl()
        {
            this.InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var scrollViewer = GetScrollViewer(TransactionList);
            if (scrollViewer != null)
            {
                scrollViewer.ViewChanged += async (s, args) =>
                {
                    if (scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight - 50)
                    {
                        var viewModel = this.DataContext as HubViewModel;
                        if (viewModel != null)
                        {
                            await viewModel.LoadMoreHistoryAsync();
                        }
                    }
                };
            }
        }

        private ScrollViewer GetScrollViewer(DependencyObject depObj)
        {
            if (depObj is ScrollViewer) return depObj as ScrollViewer;

            for (int i = 0; i < Windows.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = Windows.UI.Xaml.Media.VisualTreeHelper.GetChild(depObj, i);
                var result = GetScrollViewer(child);
                if (result != null) return result;
            }
            return null;
        }

        private void TransactionList_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as TransactionDisplayItem;
            if (item != null)
            {
                item.IsExpanded = !item.IsExpanded;
            }
        }

        private void TransactionBorder_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (e.HoldingState == Windows.UI.Input.HoldingState.Started)
            {
                FrameworkElement element = sender as FrameworkElement;
                if (element != null)
                {
                    FlyoutBase.ShowAttachedFlyout(element);
                    e.Handled = true;
                }
            }
        }

        private void EditTransaction_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuFlyoutItem;
            var displayItem = menuItem.DataContext as TransactionDisplayItem;
            if (displayItem != null)
            {
                var frame = Window.Current.Content as Frame;
                if (frame != null)
                {
                    frame.Navigate(typeof(QuickAddPage), displayItem.Id);
                }
            }
        }

        private async void DeleteTransaction_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuFlyoutItem;
            var displayItem = menuItem.DataContext as TransactionDisplayItem;
            if (displayItem != null)
            {
                var dialog = new Windows.UI.Popups.MessageDialog("are you sure you want to delete this transaction?", "confirm delete");
                dialog.Commands.Add(new Windows.UI.Popups.UICommand("delete") { Id = 0 });
                dialog.Commands.Add(new Windows.UI.Popups.UICommand("cancel") { Id = 1 });
                dialog.DefaultCommandIndex = 1;

                var result = await dialog.ShowAsync();
                if ((int)result.Id == 0)
                {
                    var transactionViewModel = new TransactionViewModel();
                    if (await transactionViewModel.DeleteTransactionAsync(displayItem.Id))
                    {
                        HubViewModel.IsDirty = true;
                        TransactionDeleted?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }
    }
}
