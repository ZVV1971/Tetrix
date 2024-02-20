using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace TetrisFigures.Dialogs
{
    /// <summary>
    /// Interaction logic for HighScoresDialog.xaml
    /// </summary>
    public partial class HighScoresDialog : Window
    {
        public HighScoresDialog(IEnumerable<Tuple<string, int, int, DateTime, string>> l)
        {
            InitializeComponent();
            DataContext = this;

            int k = 0;
            foreach (Tuple<string, int, int, DateTime, string> item in l)
            {
                if (++k > 10) break;
                TextBlock tg = (TextBlock)FindName("Gamer" + k.ToString());
                tg.Text = item.Item1;
                tg.ToolTip = $"Level={item.Item3}; {item.Item4}; {item.Item5}";
                TextBlock tv = (TextBlock)FindName("Score" + k.ToString());
                tv.Text = item.Item2.ToString();
            }
        }
    }
}
