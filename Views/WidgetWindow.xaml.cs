using System.Windows;
using System.Windows.Input;

namespace NitroOptimizer.Views
{
    public partial class WidgetWindow : Window
    {
        public WidgetWindow()
        {
            InitializeComponent();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch { }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
