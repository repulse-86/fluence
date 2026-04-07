using Fluence.Common;
using Fluence.ViewModels;
using Fluence.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Fluence
{
    public sealed partial class HubPage : Page
    {
        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");
        private readonly CategoryViewModel categoryViewModel = new CategoryViewModel();
        private readonly HubViewModel hubViewModel = new HubViewModel();

        public HubPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            this.DefaultViewModel["CategoryViewModel"] = this.categoryViewModel;
            this.DefaultViewModel["HubViewModel"] = this.hubViewModel;
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            await this.hubViewModel.LoadHistoryAsync();
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        private void AddCategoryAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Frame.Navigate(typeof(CategoryPage)))
            {
                throw new Exception("Failed to navigate to category form.");
            }
        }

        private void QuickAddAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Frame.Navigate(typeof(QuickAddPage)))
            {
                throw new Exception("Failed to navigate to quick add page.");
            }
        }

        private void TransactionList_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as TransactionDisplayItem;
            if (item != null)
            {
                item.IsExpanded = !item.IsExpanded;
            }
        }

        private void EditProfileAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Frame.Navigate(typeof(ProfilePage)))
            {
                throw new Exception("Failed to navigate to category form.");
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }
    }
}
