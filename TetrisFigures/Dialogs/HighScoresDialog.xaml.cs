using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using TetrisFigures.Auxiliary;

namespace TetrisFigures.Dialogs
{
    /// <summary>
    /// Interaction logic for HighScoresDialog.xaml
    /// </summary>
    public partial class HighScoresDialog : Window
    {
        public HighScoresDialog(IEnumerable<ScoreRecord> l)
        {
            InitializeComponent();
            DataContext = this;

            int k = 0;
            foreach (ScoreRecord item in l)
            {
                if (++k > 10) break;
                TextBlock tg = (TextBlock)FindName("Gamer" + k.ToString());
                tg.Text = item.playerName;
                tg.ToolTip = $"Level={item.level}; {item.recordTimestamp}; {item.gameFieldSize}; {item.complexityLevel}";
                TextBlock tv = (TextBlock)FindName("Score" + k.ToString());
                tv.Text = item.score.ToString();
            }
        }
    }
}
