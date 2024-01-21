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
        public HighScoresDialog(List<Tuple<string, int, int, DateTime>> l)
        {
            InitializeComponent();
            DataContext = this;

            int k = 0;
            foreach (Tuple<string, int, int, DateTime> item in l)
            {
                if (++k > 10) break;
                TextBlock tg = (TextBlock)this.FindName("Gamer" + k.ToString());
                tg.Text = item.Item1;
                tg.ToolTip = $"Level={item.Item3}; {item.Item4}";
                TextBlock tv = (TextBlock)this.FindName("Score" + k.ToString());
                tv.Text = item.Item2.ToString();
            }
        }
    }
}
