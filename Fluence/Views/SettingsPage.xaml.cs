using Fluence.Common;
using Fluence.Models;
using Fluence.ViewModels;
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
        private ProfileViewModel _profileViewModel;
        private WalletViewModel _walletViewModel;
        private SystemViewModel _systemViewModel;
        private readonly NavigationHelper navigationHelper;

        public CategoryViewModel CategoryVM => _categoryViewModel;
        public ProfileViewModel ProfileVM => _profileViewModel;
        public WalletViewModel WalletVM => _walletViewModel;
        public SystemViewModel SystemVM => _systemViewModel;

        public SettingsPage()
        {
            this.InitializeComponent();
            _categoryViewModel = new CategoryViewModel();
            _profileViewModel = new ProfileViewModel();
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
                else if (target == "wallet")
                {
                    SettingsPivot.SelectedItem = WalletPivotItem;
                    await _walletViewModel.LoadWalletsAsync();
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
            if (SettingsPivot.SelectedItem == CategoryPivotItem)
            {
                PrimaryAppBarButton.Label = _categoryViewModel.SaveButtonLabel;
                PrimaryAppBarButton.Icon = new SymbolIcon(_categoryViewModel.SaveButtonSymbol);
            }
            else if (SettingsPivot.SelectedItem == ProfilePivotItem)
            {
                PrimaryAppBarButton.Label = _profileViewModel.SaveButtonLabel;
                PrimaryAppBarButton.Icon = new SymbolIcon(_profileViewModel.SaveButtonSymbol);
            }
            else if (SettingsPivot.SelectedItem == WalletPivotItem)
            {
                PrimaryAppBarButton.Label = "save";
                PrimaryAppBarButton.Icon = new SymbolIcon(Symbol.Save);
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
                if (_categoryViewModel.IsBusy) return;

                await _categoryViewModel.SaveCategoryAsync();
                HubViewModel.IsDirty = true;
                
                CategoryControl.CategoryNameInput.IsEnabled = false;
                this.Focus(FocusState.Programmatic);
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    CategoryControl.CategoryNameInput.IsEnabled = true;
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
                        HubViewModel.IsDirty = true;
                        _profileViewModel.IsEditing = false;
                    }
                }
                UpdateAppBar();
            }
            else if (SettingsPivot.SelectedItem == WalletPivotItem)
            {
                if (_walletViewModel.IsBusy) return;

                double balance;
                double.TryParse(_walletViewModel.WalletBalance, out balance);

                var wallet = _walletViewModel.SelectedWallet ?? new Wallet();
                wallet.Name = _walletViewModel.WalletName;
                wallet.Balance = balance;

                await _walletViewModel.SaveWalletAsync(wallet);
                _walletViewModel.SelectedWallet = null;
                _walletViewModel.WalletName = string.Empty;
                _walletViewModel.WalletBalance = string.Empty;
                HubViewModel.IsOverviewDirty = true;
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
            if (this.Frame != null && this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }

        private void CategoryControl_EditRequested(object sender, EventArgs e)
        {
            UpdateAppBar();
        }

        private async void SystemControl_DataChanged(object sender, EventArgs e)
        {
            await _categoryViewModel.LoadCategoriesAsync();
            await _profileViewModel.LoadProfileDetailsAsync();
            await _walletViewModel.LoadWalletsAsync();
            UpdateAppBar();
        }
    }
}
