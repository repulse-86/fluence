using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Fluence.Views.Components.Reports
{
    public sealed partial class ReportDistributionControl : UserControl
    {
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(ReportDistributionControl), new PropertyMetadata(null));

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public ReportDistributionControl()
        {
            this.InitializeComponent();
        }
    }
}
