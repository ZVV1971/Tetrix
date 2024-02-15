using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TetrisFigures.Interfaces;

namespace TetrisFigures.Dialogs
{
    /// <summary>
    /// Interaction logic for InfoDialog.xaml
    /// </summary>
    public partial class InfoDialog : Window
    {
        private int rotationAngle;
        private TetrisUserControl ctrl;

        public InfoDialog()
        {
            InitializeComponent();
            rotationAngle = 0;
        }

        private void Window_ContentRendered(object sender, System.EventArgs e)
        {
            ctrl = new TetrisCapControl();
            ctrl.ChangeSize(30);
            ctrl.color = new SolidColorBrush(Colors.Red);
            Canvas.SetLeft(ctrl, ShowCase.ActualWidth / 2 - ctrl.Width / 2);
            Canvas.SetBottom(ctrl, ShowCase.ActualHeight / 2 - ctrl.Height / 2);
            ShowCase.Children.Add(ctrl);
        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            rotationAngle = (rotationAngle + 90) % 360;
            ctrl.LayoutTransform = new RotateTransform(rotationAngle);
        }
    }
}
