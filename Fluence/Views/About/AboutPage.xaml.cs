using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Fluence.Common;

namespace Fluence.Views.About
{
    public sealed partial class AboutPage : Page
    {
        private readonly NavigationHelper navigationHelper;

        public AboutPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
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
