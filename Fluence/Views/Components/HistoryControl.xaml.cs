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
                    var viewModel = this.DataContext as HubViewModel;
                    if (viewModel != null && viewModel.IsActive && scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight - 50)
                    {
                        await viewModel.LoadMoreHistoryAsync();
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
                var frame = Window.Current.Content as Frame;
                if (frame != null)
                {
                    frame.Navigate(typeof(TransactionDetailsPage), item.Id);
                }
            }
        }

    }
}
