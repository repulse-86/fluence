using Fluence.Common;
using Fluence.Models;
using Fluence.ViewModels;
using Fluence.Views;
using System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Fluence.Views.Management
{
    public sealed partial class SettingsPage : Page
    {
        private CategoryViewModel _categoryViewModel;
        private LimitsViewModel _limitsViewModel;
        private WalletViewModel _walletViewModel;
        private SystemViewModel _systemViewModel;
        private readonly NavigationHelper navigationHelper;
        private int _lastPivotIndex;

        public CategoryViewModel CategoryVM => _categoryViewModel;
        public LimitsViewModel LimitsVM => _limitsViewModel;
        public WalletViewModel WalletVM => _walletViewModel;
        public SystemViewModel SystemVM => _systemViewModel;

        public SettingsPage()
        {
            this.InitializeComponent();
            _categoryViewModel = new CategoryViewModel();
            _limitsViewModel = new LimitsViewModel();
            _walletViewModel = new WalletViewModel();
            _systemViewModel = new SystemViewModel();
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
            await _limitsViewModel.LoadLimitsDetailsAsync();
            await _walletViewModel.LoadWalletsAsync();

            if (e.Parameter != null)
            {
                string target = e.Parameter.ToString().ToLower();
                if (target == "category")
                {
                    SettingsPivot.SelectedItem = CategoryPivotItem;
                }
                else if (target == "limits")
                {
                    SettingsPivot.SelectedItem = LimitsPivotItem;
                }
                else if (target == "wallet")
                {
                    SettingsPivot.SelectedItem = WalletPivotItem;
                }
            }
            else if (e.NavigationMode == NavigationMode.Back)
            {
                if (_lastPivotIndex >= 0 && _lastPivotIndex < SettingsPivot.Items.Count)
                {
                    SettingsPivot.SelectedIndex = _lastPivotIndex;
                }
            }
            UpdateAppBar();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _lastPivotIndex = SettingsPivot.SelectedIndex;
            this.navigationHelper.OnNavigatedFrom(e);
        }

        private void SettingsPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateAppBar();
        }

        private void UpdateAppBar()
        {
            if (SettingsPivot.SelectedItem == CategoryPivotItem)
            {
                PrimaryAppBarButton.Label = _categoryViewModel.SaveButtonLabel;
                PrimaryAppBarButton.Icon = new SymbolIcon(_categoryViewModel.SaveButtonSymbol);
            }
            else if (SettingsPivot.SelectedItem == LimitsPivotItem)
            {
                PrimaryAppBarButton.Label = _limitsViewModel.SaveButtonLabel;
                PrimaryAppBarButton.Icon = new SymbolIcon(_limitsViewModel.SaveButtonSymbol);
            }
            else if (SettingsPivot.SelectedItem == WalletPivotItem)
            {
                PrimaryAppBarButton.Label = _walletViewModel.SaveButtonLabel;
                PrimaryAppBarButton.Icon = new SymbolIcon(_walletViewModel.SaveButtonSymbol);
            }
            else if (SettingsPivot.SelectedItem == SystemPivotItem)
            {
                PrimaryAppBarButton.Label = "save";
                PrimaryAppBarButton.Icon = new SymbolIcon(Symbol.Save);
            }
            
            SecondaryAppBarButton.Visibility = Visibility.Visible;
            SecondaryAppBarButton.Label = "cancel";
            SecondaryAppBarButton.Icon = new SymbolIcon(Symbol.Cancel);
        }

        private async void PrimaryAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (SettingsPivot.SelectedItem == CategoryPivotItem)
            {
                if (!_categoryViewModel.IsEditing)
                {
                    _categoryViewModel.IsEditing = true;
                }
                else
                {
                    if (_categoryViewModel.IsBusy) return;

                    await _categoryViewModel.SaveCategoryAsync();
                    HubViewModel.IsDirty = true;
                }
                UpdateAppBar();
            }
            else if (SettingsPivot.SelectedItem == LimitsPivotItem)
            {
                if (_limitsViewModel.IsBusy) return;

                if (!_limitsViewModel.IsEditing)
                {
                    _limitsViewModel.IsEditing = true;
                }
                else
                {
                    bool success = await _limitsViewModel.SaveLimitsDetailsAsync();
                    if (success)
                    {
                        HubViewModel.IsDirty = true;
                        _limitsViewModel.IsEditing = false;
                    }
                }
                UpdateAppBar();
            }
            else if (SettingsPivot.SelectedItem == WalletPivotItem)
            {
                if (!_walletViewModel.IsEditing)
                {
                    _walletViewModel.IsEditing = true;
                }
                else
                {
                    if (_walletViewModel.IsBusy) return;

                    await _walletViewModel.SaveWalletFromFormAsync();
                    HubViewModel.IsOverviewDirty = true;
                }
                UpdateAppBar();
            }
            else if (SettingsPivot.SelectedItem == SystemPivotItem)
            {
                _systemViewModel.SaveSettings();
                await new Windows.UI.Popups.MessageDialog("system settings saved successfully.", "success").ShowAsync();
            }
        }

        private void SecondaryAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            _categoryViewModel.IsEditing = false;
            _limitsViewModel.IsEditing = false;
            _walletViewModel.IsEditing = false;
            _categoryViewModel.CategoryName = string.Empty;
            _categoryViewModel.SelectedCategory = null;
            _walletViewModel.WalletName = string.Empty;
            _walletViewModel.WalletBalance = string.Empty;
            _walletViewModel.SelectedWallet = null;
            UpdateAppBar();
        }

        private async void SystemControl_DataChanged(object sender, EventArgs e)
        {
            await _categoryViewModel.LoadCategoriesAsync();
            await _limitsViewModel.LoadLimitsDetailsAsync();
            await _walletViewModel.LoadWalletsAsync();
            UpdateAppBar();
        }
    }
}
