using System.Windows;

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
            width = w;
            height = h;
        }

        public byte width
        {
            get { return (byte)GetValue(GameFieldWidthProperty); }
            set { SetValue(GameFieldWidthProperty, value); }
        }
        
        public byte height
        {
            get { return (byte)GetValue(GameFieldHeightProperty); }
            set { SetValue(GameFieldHeightProperty, value); }
        }

        public static readonly DependencyProperty GameFieldWidthProperty =
            DependencyProperty.Register(nameof(width),
                typeof(byte), typeof(GameFieldSizeDialog),
                new PropertyMetadata((byte)20),
                new ValidateValueCallback(validateWidth));

        public static readonly DependencyProperty GameFieldHeightProperty =
            DependencyProperty.Register(nameof(height),
                typeof(byte), typeof(GameFieldSizeDialog),
                new PropertyMetadata((byte)40),
                new ValidateValueCallback(validateHeight));

        static bool validateWidth(object value)
        {
            return (byte)value >= 10 && (byte)value <= 20;
        }

        static bool validateHeight(object value)
        {
            return (byte)value >= 20 && (byte)value <= 40;
        }
    }
}
