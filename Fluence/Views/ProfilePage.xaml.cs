using Fluence.Common;
using Fluence.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Fluence.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProfilePage : Page
    {
        private ProfileViewModel _viewModel;
        private readonly NavigationHelper navigationHelper;

        public ProfilePage()
        {
            this.InitializeComponent();
            _viewModel = new ProfileViewModel();
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

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);

            try
            {
                await _viewModel.LoadProfileDetailsAsync();
            }
            catch (Exception)
            {
            }
        }

        private async void SaveProfileAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.IsBusy) return;

            if (!_viewModel.IsEditing)
            {
                _viewModel.IsEditing = true;
            }
            else
            {
                try
                {
                    bool success = await _viewModel.SaveProfileDetailsAsync();
                    
                    if (success)
                    {
                        _viewModel.IsEditing = false;
                    }
                }
                catch (Exception) { }
            }
        }

        private void EditProfileAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.IsEditing = true;
        }
    }
}
