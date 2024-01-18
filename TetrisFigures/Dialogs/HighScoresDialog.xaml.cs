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
        public HighScoresDialog(List<KeyValuePair<string,int>> l)
        {
            InitializeComponent();
            DataContext = this;

            int k = 0;
            foreach (KeyValuePair<string, int> item in l)
            {
                if (++k > 10) break;
                TextBlock tg = (TextBlock)this.FindName("Gamer" + k.ToString());
                tg.Text = item.Key;
                TextBlock tv = (TextBlock)this.FindName("Score" + k.ToString());
                tv.Text = item.Value.ToString();
            }
        }
    }
}
