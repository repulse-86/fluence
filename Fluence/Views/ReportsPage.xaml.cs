using Fluence.Common;
using Fluence.ViewModels;
using System;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Fluence.Views
{
    public class ReportsNavigationParameter
    {
        public HubViewModel ViewModel { get; set; }
        public string Period { get; set; }
    }

    public sealed partial class ReportsPage : Page
    {
        private readonly NavigationHelper navigationHelper;

        public ReportsPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
        }

        public NavigationHelper NavigationHelper => this.navigationHelper;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
            var param = e.Parameter as ReportsNavigationParameter;
            if (param != null)
            {
                this.DataContext = param.ViewModel;

                // Repopulate data if it was cleared upon navigating away from Hub
                if (param.ViewModel != null)
                {
                    await param.ViewModel.LoadReportAsync();
                }

                string target = (param.Period ?? "today").ToLower();

                // Small delay to ensure Pivot is ready to handle selection change
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                    var items = ReportsPivot.Items.Cast<PivotItem>().ToList();
                    var targetItem = items.FirstOrDefault(i => i.Header.ToString().ToLower() == target);
                    if (targetItem != null)
                    {
                        ReportsPivot.SelectedItem = targetItem;
                    }
                });
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }
    }
}
