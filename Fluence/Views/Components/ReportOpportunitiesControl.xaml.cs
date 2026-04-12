using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Fluence.Views.Components
{
    public sealed partial class ReportOpportunitiesControl : UserControl
    {
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(ReportOpportunitiesControl), new PropertyMetadata("saving opportunities"));

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public ReportOpportunitiesControl()
        {
            this.InitializeComponent();
        }
    }
}
