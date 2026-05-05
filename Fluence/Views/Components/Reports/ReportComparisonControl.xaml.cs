using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Fluence.Views.Components.Reports
{
    public sealed partial class ReportComparisonControl : UserControl
    {
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(ReportComparisonControl), new PropertyMetadata(null));

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty TrendTextProperty =
            DependencyProperty.Register("TrendText", typeof(string), typeof(ReportComparisonControl), new PropertyMetadata("vs yesterday"));

        public string TrendText
        {
            get { return (string)GetValue(TrendTextProperty); }
            set { SetValue(TrendTextProperty, value); }
        }

        public static readonly DependencyProperty ShowProjectionsProperty =
            DependencyProperty.Register("ShowProjections", typeof(bool), typeof(ReportComparisonControl), new PropertyMetadata(false));

        public bool ShowProjections
        {
            get { return (bool)GetValue(ShowProjectionsProperty); }
            set { SetValue(ShowProjectionsProperty, value); }
        }

        public static readonly DependencyProperty AverageLabelProperty =
            DependencyProperty.Register("AverageLabel", typeof(string), typeof(ReportComparisonControl), new PropertyMetadata("daily average"));

        public string AverageLabel
        {
            get { return (string)GetValue(AverageLabelProperty); }
            set { SetValue(AverageLabelProperty, value); }
        }

        public static readonly DependencyProperty ProjectedLabelProperty =
            DependencyProperty.Register("ProjectedLabel", typeof(string), typeof(ReportComparisonControl), new PropertyMetadata("projected total"));

        public string ProjectedLabel
        {
            get { return (string)GetValue(ProjectedLabelProperty); }
            set { SetValue(ProjectedLabelProperty, value); }
        }

        public ReportComparisonControl()
        {
            this.InitializeComponent();
        }
    }
}
