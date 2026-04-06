using Fluence.Common;
using Fluence.Models;
using Fluence.ViewModels;
using System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace Fluence.Views
{
    public sealed partial class CategoryPage : Page
    {
        private CategoryViewModel _viewModel;
        private readonly NavigationHelper navigationHelper;

        public CategoryPage()
        {
            this.InitializeComponent();
            _viewModel = new CategoryViewModel();
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

            try
            {
                await _viewModel.LoadCategoriesAsync();
            }
            catch (Exception)
            {
                // Error handled by ViewModel ErrorMessage state
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.IsBusy) return;

            try
            {
                await _viewModel.SaveCategoryAsync();
            }
            catch (Exception)
            {
                // Error handled by ViewModel ErrorMessage state
            }
        }

        private void CategoryBorder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            if (element != null)
            {
                FlyoutBase.ShowAttachedFlyout(element);
            }
        }

        private async void EditCategory_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuFlyoutItem;
            Category category = menuItem.DataContext as Category;

            if (category != null)
            {
                _viewModel.SelectedCategory = category;
                _viewModel.CategoryName = category.Name;

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    CategoryTextBox.Focus(FocusState.Programmatic);
                    CategoryTextBox.SelectAll();
                });
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame != null && this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }
    }
}
