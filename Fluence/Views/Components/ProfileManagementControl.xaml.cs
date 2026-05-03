using System;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Fluence.Views.Components
{
    public sealed partial class ProfileManagementControl : UserControl
    {
        public ProfileManagementControl()
        {
            this.InitializeComponent();
        }

        private void AmountTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            bool isDigit = e.Key >= Windows.System.VirtualKey.Number0 && e.Key <= Windows.System.VirtualKey.Number9;
            bool isPadDigit = e.Key >= Windows.System.VirtualKey.NumberPad0 && e.Key <= Windows.System.VirtualKey.NumberPad9;
            bool isDecimal = e.Key == Windows.System.VirtualKey.Decimal || (int)e.Key == 190;

            if (!isDigit && !isPadDigit && !isDecimal && e.Key != Windows.System.VirtualKey.Back)
            {
                e.Handled = true;
            }

            if (isDecimal && (sender as TextBox).Text.Contains("."))
            {
                e.Handled = true;
            }
        }

        private void AmountTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            string text = tb.Text;
            string filtered = new string(text.Where(c => char.IsDigit(c) || c == '.').ToArray());

            if (text != filtered)
            {
                tb.Text = filtered;
                tb.SelectionStart = tb.Text.Length;
            }
        }
    }
}
