using System.Windows;
using System.Windows.Input;
using TetrisFigures.Auxiliary;

namespace TetrisFigures.Dialogs
{
    /// <summary>
    /// Interaction logic for GameFieldSizeDialog.xaml
    /// </summary>
    public partial class GameFieldSizeDialog : Window
    {
        /// <summary>
        /// Initiates dialog with preset values of width & height
        /// </summary>
        /// <param name="w">Width of the game field</param>
        /// <param name="h">Height of the game field</param>
        public GameFieldSizeDialog(byte w, byte h)
        {
            InitializeComponent();
            DataContext = this;
            sz = new GameGridSize()
            {
                height = h,
                width = w
            };
        }

        public GameGridSize sz
        {
            get;
            set;
        }

        private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void SaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                e.CanExecute = !sz.HasErrors;
            }
            catch 
            {
                e.CanExecute = false;
            }
        }
    }
}