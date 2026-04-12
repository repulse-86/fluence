using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Fluence.Views.Components
{
    public sealed partial class ReportRunwayControl : UserControl
    {
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(ReportRunwayControl), new PropertyMetadata("runway forecast"));

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public ReportRunwayControl()
        {
            this.InitializeComponent();
        }
    }
}
