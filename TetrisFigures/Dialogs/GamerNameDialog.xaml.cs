using System;
using System.Windows;

namespace TetrisFigures.Dialogs
{
    /// <summary>
    /// Interaction logic for GamerNameDialog.xaml
    /// </summary>
    public partial class GamerNameDialog : Window
    {
        public GamerNameDialog(string question, string defaultAnswer = "")
        {
            InitializeComponent();
            lblQuestion.Content = question;
            txtAnswer.Text = defaultAnswer;
        }

        private void btnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        public string Answer
        {
            get { return txtAnswer.Text; }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            txtAnswer.SelectAll();
            txtAnswer.Focus();
        }
    }
}
