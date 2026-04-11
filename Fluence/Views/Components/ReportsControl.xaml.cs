using Fluence.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Fluence.Views.Components
{
    public sealed partial class ReportsControl : UserControl
    {
        public ReportsControl()
        {
            this.InitializeComponent();
        }

        private void ShowMore_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var period = button.Tag as string;
            var viewModel = this.DataContext as ViewModels.HubViewModel;

            if (viewModel != null)
            {
                var frame = Window.Current.Content as Frame;
                if (frame != null)
                {
                    frame.Navigate(typeof(ReportsPage), new ReportsNavigationParameter
                    {
                        ViewModel = viewModel,
                        Period = period
                    });
                }
            }
        }
    }
}
