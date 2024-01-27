using System.Windows;
using System.Windows.Controls;

namespace TetrisFigures.Auxiliary
{
    /// <summary>
    /// Interaction logic for ScoreInfoControl.xaml
    /// </summary>
    public partial class ScoreInfoControl : UserControl
    {
        public ScoreInfoControl()
        {
            InitializeComponent();
            DataContext = this;
            movementCounter = 0;
        }
        public string txtScoreInfo { get; set; }
        public int movementCounter { get; set; }
        public Visibility visibility
        {
            get { return (Visibility)GetValue(TBVisibilityProperty); }
            set { SetValue(TBVisibilityProperty, value); }
        }

        public static readonly DependencyProperty TBVisibilityProperty =
            DependencyProperty.Register
            (
                "visibility",
                typeof(Visibility),
                typeof(ScoreInfoControl)
            );
    }
}