using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Fluence.Views.Components.Reports
{
    public sealed partial class ReportSpendingPatternsControl : UserControl
    {
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(ReportSpendingPatternsControl), new PropertyMetadata("spending patterns"));

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public ReportSpendingPatternsControl()
        {
            this.InitializeComponent();
        }
    }
}
