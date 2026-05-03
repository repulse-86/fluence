using Fluence.Models;
using Fluence.ViewModels;
using System;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace Fluence.Views.Components
{
    public sealed partial class CategoryManagementControl : UserControl
    {
        public event EventHandler EditRequested;

        public CategoryManagementControl()
        {
            this.InitializeComponent();
        }

        public TextBox CategoryNameInput => CategoryTextBox;

        private void CategoryBorder_Holding(object sender, HoldingRoutedEventArgs e)
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

        private async void EditCategory_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuFlyoutItem;
            Category category = menuItem.DataContext as Category;
            var viewModel = this.DataContext as CategoryViewModel;

            if (category != null && viewModel != null)
            {
                viewModel.SelectedCategory = category;
                viewModel.CategoryName = category.Name;

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    CategoryTextBox.Focus(FocusState.Programmatic);
                    CategoryTextBox.SelectAll();
                });

                EditRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        private async void DeleteCategory_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuFlyoutItem;
            Category category = menuItem.DataContext as Category;
            var viewModel = this.DataContext as CategoryViewModel;

            if (category != null && viewModel != null)
            {
                var dialog = new MessageDialog(
                    $"Are you sure you want to delete '{category.Name}'?", "confirm delete");

                dialog.Commands.Add(new UICommand("delete") { Id = 0 });
                dialog.Commands.Add(new UICommand("cancel") { Id = 1 });
                dialog.DefaultCommandIndex = 1;

                var result = await dialog.ShowAsync();
                if ((int)result.Id == 0)
                {
                    CategoryTextBox.IsReadOnly = true;
                    CategoryTextBox.IsEnabled = false;

                    await viewModel.DeleteCategoryAsync(category);
                    HubViewModel.IsDirty = true;

                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        this.Focus(FocusState.Programmatic);
                        CategoryTextBox.IsEnabled = true;
                        CategoryTextBox.IsReadOnly = false;
                    });
                }
            }
        }
    }
}
