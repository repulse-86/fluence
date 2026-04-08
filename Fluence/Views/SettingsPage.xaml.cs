using Fluence.Common;
using Fluence.Models;
using Fluence.ViewModels;
using Fluence.Services;
using System;
using System.Linq;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace Fluence.Views
{
    public sealed partial class SettingsPage : Page
    {
        private CategoryViewModel _categoryViewModel;
        private ProfileViewModel _profileViewModel;
        private readonly NavigationHelper navigationHelper;

        public CategoryViewModel CategoryVM => _categoryViewModel;
        public ProfileViewModel ProfileVM => _profileViewModel;

        public SettingsPage()
        {
            this.InitializeComponent();
            _categoryViewModel = new CategoryViewModel();
            _profileViewModel = new ProfileViewModel();
            this.DataContext = this;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        public NavigationHelper NavigationHelper => this.navigationHelper;

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);

            await _categoryViewModel.LoadCategoriesAsync();
            await _profileViewModel.LoadProfileDetailsAsync();

            if (e.Parameter != null)
            {
                string target = e.Parameter.ToString().ToLower();
                if (target == "category")
                {
                    SettingsPivot.SelectedItem = CategoryPivotItem;
                }
                else if (target == "profile")
                {
                    SettingsPivot.SelectedItem = ProfilePivotItem;
                }
            }
            UpdateAppBar();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        private void SettingsPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateAppBar();
        }

        private void UpdateAppBar()
        {
            PrimaryAppBarButton.Label = (SettingsPivot.SelectedItem == CategoryPivotItem) ? _categoryViewModel.SaveButtonLabel : _profileViewModel.SaveButtonLabel;
            PrimaryAppBarButton.Icon = new SymbolIcon((SettingsPivot.SelectedItem == CategoryPivotItem) ? _categoryViewModel.SaveButtonSymbol : _profileViewModel.SaveButtonSymbol);
            
            SecondaryAppBarButton.Visibility = Visibility.Visible;
            SecondaryAppBarButton.Label = "cancel";
            SecondaryAppBarButton.Icon = new SymbolIcon(Symbol.Cancel);
        }

        private async void PrimaryAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (SettingsPivot.SelectedItem == CategoryPivotItem)
            {
                if (_categoryViewModel.IsBusy) return;

                await _categoryViewModel.SaveCategoryAsync();
                CategoryTextBox.IsEnabled = false;
                this.Focus(FocusState.Programmatic);
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    CategoryTextBox.IsEnabled = true;
                    Windows.UI.ViewManagement.InputPane.GetForCurrentView().TryHide();
                });
                UpdateAppBar();
            }
            else if (SettingsPivot.SelectedItem == ProfilePivotItem)
            {
                if (_profileViewModel.IsBusy) return;

                if (!_profileViewModel.IsEditing)
                {
                    _profileViewModel.IsEditing = true;
                }
                else
                {
                    bool success = await _profileViewModel.SaveProfileDetailsAsync();
                    if (success)
                    {
                        _profileViewModel.IsEditing = false;
                    }
                }
                UpdateAppBar();
            }
        }

        private void SecondaryAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame != null && this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }

        // Category Page logic
        private void CategoryBorder_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (e.HoldingState == HoldingState.Started)
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

            if (category != null)
            {
                _categoryViewModel.SelectedCategory = category;
                _categoryViewModel.CategoryName = category.Name;

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    CategoryTextBox.Focus(FocusState.Programmatic);
                    CategoryTextBox.SelectAll();
                });
                UpdateAppBar();
            }
        }

        private async void DeleteCategory_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuFlyoutItem;
            Category category = menuItem.DataContext as Category;

            if (category != null)
            {
                var dialog = new Windows.UI.Popups.MessageDialog(
                    $"Are you sure you want to delete '{category.Name}'?", "confirm delete");

                dialog.Commands.Add(new Windows.UI.Popups.UICommand("delete") { Id = 0 });
                dialog.Commands.Add(new Windows.UI.Popups.UICommand("cancel") { Id = 1 });
                dialog.DefaultCommandIndex = 1;

                var result = await dialog.ShowAsync();
                if ((int)result.Id == 0)
                {
                    CategoryTextBox.IsReadOnly = true;
                    CategoryTextBox.IsEnabled = false;

                    await _categoryViewModel.DeleteCategoryAsync(category);

                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        this.Focus(FocusState.Programmatic);
                        CategoryTextBox.IsEnabled = true;
                        CategoryTextBox.IsReadOnly = false;
                    });
                }
            }
        }

        // Profile Page logic
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

        // System Page logic
        private async void MockData_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Windows.UI.Popups.MessageDialog("this will PERMANENTLY delete all your current transactions and replace them with 30 days of mock data. proceed?", "confirm mock data");
            dialog.Commands.Add(new Windows.UI.Popups.UICommand("proceed") { Id = 0 });
            dialog.Commands.Add(new Windows.UI.Popups.UICommand("cancel") { Id = 1 });
            dialog.DefaultCommandIndex = 1;

            var result = await dialog.ShowAsync();
            if ((int)result.Id == 0)
            {
                var categoryService = new CategoryService();
                await categoryService.InitializeDatabaseSync();

                var transactionService = new TransactionService();
                await transactionService.SeedTransactionsAsync();

                var profileService = new ProfileService();
                await profileService.SeedProfileAsync();

                await _categoryViewModel.LoadCategoriesAsync();
                await _profileViewModel.LoadProfileDetailsAsync();
                UpdateAppBar();

                await new Windows.UI.Popups.MessageDialog("mock data activated successfully.", "success").ShowAsync();
            }
        }

        private async void ClearData_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Windows.UI.Popups.MessageDialog("this will PERMANENTLY delete ALL-TIME transactions and reset your profile settings. this cannot be undone. proceed?", "DANGER: CLEAR ALL DATA");
            dialog.Commands.Add(new Windows.UI.Popups.UICommand("clear everything") { Id = 0 });
            dialog.Commands.Add(new Windows.UI.Popups.UICommand("cancel") { Id = 1 });
            dialog.DefaultCommandIndex = 1;

            var result = await dialog.ShowAsync();
            if ((int)result.Id == 0)
            {
                var transactionService = new TransactionService();
                await transactionService.ClearTransactionsAsync();

                var profileService = new ProfileService();
                await profileService.ClearProfileAsync();

                await _categoryViewModel.LoadCategoriesAsync();
                await _profileViewModel.LoadProfileDetailsAsync();
                UpdateAppBar();

                await new Windows.UI.Popups.MessageDialog("all data cleared successfully.", "success").ShowAsync();
            }
        }
    }
}
