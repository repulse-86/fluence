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
    public sealed partial class CategoryManagementControl : UserControl
    {
        public CategoryManagementControl()
        {
            this.InitializeComponent();
        }

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

        private void EditCategory_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuFlyoutItem;
            Category category = menuItem.DataContext as Category;
            var viewModel = this.DataContext as CategoryViewModel;

            if (category != null && viewModel != null)
            {
                viewModel.SelectedCategory = category;
                viewModel.CategoryName = category.Name;
                viewModel.IsEditing = true;
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
                    await viewModel.DeleteCategoryAsync(category);
                    HubViewModel.IsDirty = true;
                }
            }
        }
    }
}
