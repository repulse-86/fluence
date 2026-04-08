using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Fluence.Views.Components
{
    public sealed partial class ReportComparisonControl : UserControl
    {
        public static readonly DependencyProperty TrendTextProperty =
            DependencyProperty.Register("TrendText", typeof(string), typeof(ReportComparisonControl), new PropertyMetadata("vs yesterday"));

        public string TrendText
        {
            get { return (string)GetValue(TrendTextProperty); }
            set { SetValue(TrendTextProperty, value); }
        }

        public ReportComparisonControl()
        {
            this.InitializeComponent();
        }
    }
}
