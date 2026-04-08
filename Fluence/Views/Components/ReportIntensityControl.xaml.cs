using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Fluence.Views.Components
{
    public sealed partial class ReportIntensityControl : UserControl
    {
        public static readonly DependencyProperty IsYearlyProperty =
            DependencyProperty.Register("IsYearly", typeof(bool), typeof(ReportIntensityControl), new PropertyMetadata(false, OnIsYearlyChanged));

        public bool IsYearly
        {
            get { return (bool)GetValue(IsYearlyProperty); }
            set { SetValue(IsYearlyProperty, value); }
        }

        public ReportIntensityControl()
        {
            this.InitializeComponent();
            this.Loaded += (s, e) => UpdateState();
        }

        private static void OnIsYearlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ReportIntensityControl)d).UpdateState();
        }

        private void UpdateState()
        {
            VisualStateManager.GoToState(this, IsYearly ? "Yearly" : "Monthly", true);
        }
    }
}
